using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using Samples.Players.DataAccess.Models;

namespace Samples.Players.DataAccess;

public static class PlayersMigrationExtensions
{
    public static async Task<bool> MigrateDataStoresAsync(this IServiceProvider builtServiceProvider)
    {
        var logFactory = builtServiceProvider.GetRequiredService<ILoggerFactory>();
        var log = logFactory.CreateLogger("DataStoreMigration");

        log.LogInformation("Starting Player data migration");

        var createdPlayers = await ConfigureEsIndexesAsync<EsPlayer>(builtServiceProvider, EsIndexes.PlayersIndex);
        var createdPositions = await ConfigureEsIndexesAsync<EsSportPosition>(builtServiceProvider, EsIndexes.SportPositionsIndex);

        log.LogInformation("Player data migration complete");

        return createdPlayers || createdPositions;
    }

    private static async Task<bool> ConfigureEsIndexesAsync<T>(IServiceProvider provider, string indexName)
        where T : class
    {
        var esClient = provider.GetRequiredService<IElasticClient>();

        var indexExists = await esClient.Indices.ExistsAsync(indexName);

        if (indexExists?.Exists ?? false)
        {
            return false;
        }

        var indexCreateResponse = await esClient.Indices.CreateAsync(indexName,
                                                                     cid => cid.Map<T>(d => d.AutoMap()
                                                                                             .Dynamic(false))
                                                                               .IncludeTypeName(false)
                                                                               .Settings(s => s.NumberOfShards(5)
                                                                                               .NumberOfReplicas(0)
                                                                                               .RefreshInterval(new Time(TimeSpan.FromSeconds(3)))
                                                                                               .UnassignedNodeLeftDelayedTimeout(new Time(TimeSpan.FromMinutes(13)))
                                                                                               .Analysis(ad => ad.Analyzers(az => az.Language("default", l => l.Language(Language.English)))
                                                                                                                 .Normalizers(nd => nd.Custom("samplekeyword", kw => kw.Filters("lowercase", "asciifolding"))))));

        if (!indexCreateResponse.Successful())
        {
            throw indexCreateResponse.ToException();
        }
        
        return true;
    }
}
