using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Samples.Players.Models;

namespace Samples.Players.Services;

public class CbsDemoDataService : IDemoDataService
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<CbsDemoDataService> _log;

    public CbsDemoDataService(IPlayerService playerService, ILogger<CbsDemoDataService> log)
    {
        _playerService = playerService;
        _log = log;
    }
    
    public async Task CreateDemoDataAsync()
    {
        // Very naive implementation...
        _log.LogInformation("Demo data creation starting");
        
        var httpClient = new HttpClient();

        var tasks = new[]
                    {
                        GetAndUpsertPlayerDataAsync(httpClient, "baseball"),
                        GetAndUpsertPlayerDataAsync(httpClient, "football"),
                        GetAndUpsertPlayerDataAsync(httpClient, "basketball"),
                    };

        Task.WaitAll(tasks);

        _log.LogInformation("Demo data creation complete");
    }

    private async Task GetAndUpsertPlayerDataAsync(HttpClient httpClient, string sport)
    {
        _log.LogInformation("Starting demo data fetch and upsert for {Sport}", sport);
        
        var response = await httpClient.GetFromJsonAsync<CbsPlayersResponse>(GetUrl(sport));

        await _playerService.UpsertPlayersAsync(response.Body.Players
                                                        .Select(p => new Player
                                                                     {
                                                                         Position = p.Position,
                                                                         Sport = sport,
                                                                         LastName = p.LastName,
                                                                         FirstName = p.FirstName,
                                                                         Age = p.Age,
                                                                         Id = p.Id
                                                                     }));
        
        _log.LogInformation("Finished demo data fetch and upsert for {Sport}", sport);
    }
    
    private string GetUrl(string sport)
        => $"http://api.cbssports.com/fantasy/players/list?version=3.0&SPORT={sport}&response_format=JSON";
    
    private class CbsPlayersResponse
    {
        public string StatusMessage { get; set; }
        public int StatusCode { get; set; }
        public string Uri { get; set; }
        public string UriAlias { get; set; }
        public CbsPlayersBody Body { get; set; }
    }

    private class CbsPlayersBody
    {
        public IReadOnlyList<CbsPlayer> Players { get; set; }
    }
    
    private class CbsPlayer
    {
        public string Photo { get; set; }
        public string Position { get; set; }
        public string Jersey { get; set; }
        public string Sport { get; set; }

        [JsonPropertyName("eligible_for_offense_and_defense")]
        public int EligibleForOffenseAndDefense { get; set; }

        [JsonPropertyName("lastname")]
        public string LastName { get; set; }

        [JsonPropertyName("fullname")]
        public string FullName { get; set; }

        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }

        public string Bats { get; set; }

        public int Age { get; set; }

        [JsonPropertyName("pro_team")]
        public string ProTeam { get; set; }

        public string Id { get; set; }

        [JsonPropertyName("pro_status")]
        public string ProStatus { get; set; }

        public string Throws { get; set; }

        [JsonPropertyName("elias_id")]
        public string EliasId { get; set; }
    }
}
