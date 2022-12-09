using Microsoft.Extensions.Logging;
using Nest;
using Samples.Players.DataAccess.Models;
using Samples.Players.Models;
using Samples.Players.Services;

namespace Samples.Players.DataAccess;

public class SyncronousEsPlayerService : IPlayerService
{
    private readonly IElasticClient _elasticClient;
    private readonly ITransform<Player, EsPlayer> _playerTransformer;
    private readonly ILogger<SyncronousEsPlayerService> _log;

    public SyncronousEsPlayerService(IElasticClient elasticClient,
                                     ITransform<Player, EsPlayer> playerTransformer,
                                     ILogger<SyncronousEsPlayerService> log)
    {
        _elasticClient = elasticClient;
        _playerTransformer = playerTransformer;
        _log = log;
    }

    public async Task<Player> GetPlayerById(string id)
    {
        var playerResponse = await _elasticClient.GetAsync(EsPlayer.GetDocumentPath(id));
                            
        var player = _playerTransformer.Transform(playerResponse.Source);

        var sportPosition = await _elasticClient.GetAsync(EsSportPosition.GetDocumentPath(player.Sport, player.Position));

        player.AvgPositionAgeDiff = Math.Round(player.Age - sportPosition.Source.AverageAge, 2);
        
        return player;
    }

    public async Task<IReadOnlyList<Player>> QueryPlayersAsync(QueryPlayers query)
    {
        IEnumerable<Func<QueryContainerDescriptor<EsPlayer>, QueryContainer>> getFilters()
        {
            if (!query.Sport.IsNullOrEmpty())
            {
                yield return f => f.Term(t => t.Sport, query.Sport);
            }
            
            if (!query.Position.IsNullOrEmpty())
            {
                yield return f => f.Term(t => t.Position, query.Position);
            }
            
            if (!query.LastNameInitial.IsNullOrEmpty())
            {
                yield return f => f.Term(t => t.LastNameInitial, query.LastNameInitial);
            }

            var minAge = query.Age > 0
                             ? query.Age
                             : query.MinAge;

            var maxAge = query.Age > 0
                             ? query.Age
                             : minAge > 0 && query.MaxAge <= 0
                                 ? int.MaxValue
                                 : query.MaxAge;
            
            if (minAge > 0 || maxAge > 0)
            {
                yield return f => f.LongRange(r => r.Field("age")
                                                    .GreaterThanOrEquals(minAge)
                                                    .LessThanOrEquals(maxAge));
            }
        }

        var search = new SearchDescriptor<EsPlayer>().Index(EsIndexes.PlayersIndex)
                                                     .Query(q => q.Bool(b => b.Filter(getFilters())))
                                                     .From(query.Skip)
                                                     .Size(query.Take)
                                                     .Sort(s => s.Ascending(a => a.Age)
                                                                 .Ascending(a => a.PlayerId));

        var response = await _elasticClient.SearchAsync<EsPlayer>(search);

        return response.Hits
                       .Select(h => _playerTransformer.Transform(h.Source))
                       .AsListReadOnly();
    }
    
    public async Task UpsertPlayersAsync(IEnumerable<Player> players)
    {
        var updatedSportPositions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        
        // Super naive and simple synronous iterative approach for simplicity sake
        foreach (var player in players)
        {
            var esPlayer = _playerTransformer.Transform(player);

            if (!updatedSportPositions.ContainsKey(esPlayer.Sport))
            {
                updatedSportPositions[esPlayer.Sport] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            updatedSportPositions[esPlayer.Sport].Add(esPlayer.Position);
            
            await _elasticClient.IndexAsync(esPlayer, idx => idx.Index(EsIndexes.PlayersIndex)
                                                                .Id(esPlayer.PlayerId));
        }
        
        // Background, fire and forget for now...
#pragma warning disable CS4014
        UpdateAgeAverages(updatedSportPositions);
#pragma warning restore CS4014
    }

    public async Task UpdateAgeAverages(IReadOnlyDictionary<string, HashSet<string>> sportPositions)
    {
        await _elasticClient.Indices.RefreshAsync(EsIndexes.PlayersIndex);
        
        foreach (var (sport, positions) in sportPositions)
        {
            foreach (var position in positions)
            {
                _log.LogInformation("Starting AvgAgeUpdate for {Sport}.{Position}", sport, position);

                var aggs = await _elasticClient.SearchAsync<EsPlayer>(d => d.Index(EsIndexes.PlayersIndex)
                                                                            .Query(q => q.Bool(b => b.Filter(bq => bq.Term(x => x.Sport, sport),
                                                                                                             bq => bq.Term(x => x.Position, position),
                                                                                                             bq => bq.LongRange(r => r.Field("age")
                                                                                                                                      .GreaterThan(10)
                                                                                                                                      .LessThan(100)))))
                                                                            .Size(0)
                                                                            .Aggregations(a => a.Average("avgage", v => v.Field(f => f.Age))));

                var avgAge = aggs.Aggregations.Average("avgage");

                var esSportPosition = new EsSportPosition
                                      {
                                          SportPositionId = EsSportPosition.GetId(sport, position),
                                          Sport = sport,
                                          Position = position,
                                          AverageAge = avgAge.Value ?? 0,
                                          PlayerCount = aggs.Total,
                                      };

                await _elasticClient.IndexAsync(esSportPosition, idx => idx.Index(EsIndexes.SportPositionsIndex)
                                                                           .Id(esSportPosition.SportPositionId));
                
                _log.LogInformation("Finished AvgAgeUpdate for {Sport}.{Position}", sport, position);
            }
        }

        _log.LogInformation("UpdateAgeAverages complete");
    }
}

