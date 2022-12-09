using Samples.Players.Models;

namespace Samples.Players.Services;

public interface IPlayerService
{
    Task UpsertPlayersAsync(IEnumerable<Player> players);

    Task<IReadOnlyList<Player>> QueryPlayersAsync(QueryPlayers query);
    Task<Player> GetPlayerById(string id);
}
