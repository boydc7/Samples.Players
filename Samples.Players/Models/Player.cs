using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Samples.Players.Services;

namespace Samples.Players.Models;

public class PostPlayers
{
    [Required]
    [MinLength(1)]
    public IReadOnlyList<Player> Players { get; set; }
}

public class QueryPlayers
{
    public string Sport { get; set; }
    public string LastNameInitial { get; set; }
    public string Position { get; set; }

    [Range(0, 100)]
    public int MinAge { get; set; }

    [Range(0, 100)]
    public int MaxAge { get; set; }

    [Range(0, 100)]
    public int Age { get; set; }

    [Range(0, 250000)]
    public int Skip { get; set; } = 0;

    [Range(1, 1000)]
    public int Take { get; set; } = 100;
}

public class Player : INameBriefSource
{
    public string Position { get; set; }
    public string Sport { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("name_brief")]
    public string NameBrief { get; set; }

    public int Age { get; set; }

    [JsonPropertyName("average_position_age_diff")]
    public double AvgPositionAgeDiff { get; set; }

    [Required]
    [MinLength(1)]
    public string Id { get; set; }
}
