using Microsoft.AspNetCore.Mvc;
using Samples.Players.Models;
using Samples.Players.Services;

namespace Samples.Players.Controllers;

public class PlayersController : SamplePlayerControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SampleApiResult<Player>>> Get(string id)
    {
        var player = await _playerService.GetPlayerById(id);

        return player.AsOkSampleApiResult();
    }

    [HttpGet("query")]
    public async Task<ActionResult<SampleApiResult<Player>>> Get([FromQuery] QueryPlayers query)
    {
        var players = await _playerService.QueryPlayersAsync(query);

        return players.AsOkSampleApiResults();
    }

    [HttpPost]
    public async Task<NoContentResult> Post(string sport, [FromBody] PostPlayers request)
    {
        await _playerService.UpsertPlayersAsync(request.Players.Select(p =>
                                                                       {
                                                                           p.Sport = sport;

                                                                           return p;
                                                                       }));

        return NoContent();
    }
}
