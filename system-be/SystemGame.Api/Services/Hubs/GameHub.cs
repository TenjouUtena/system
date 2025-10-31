using Microsoft.AspNetCore.SignalR;
using SystemGame.Api.Models;

namespace SystemGame.Api.Services.Hubs;

public class GameHub : Hub
{
    public async Task JoinGame(int gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
    }

    public async Task LeaveGame(int gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game-{gameId}");
    }

    // Agent events (Phase 6)
    public async Task SendAgentUpdate(int gameId, AgentDto agent)
    {
        await Clients.Group($"game-{gameId}").SendAsync("AgentUpdated", agent);
    }

    public async Task SendAgentLog(int gameId, int agentId, AgentLogDto log)
    {
        await Clients.Group($"game-{gameId}").SendAsync("AgentLogReceived", agentId, log);
    }

    // Spaceship events (Phase 7)
    public async Task SendSpaceshipUpdate(int gameId, SpaceshipDto spaceship)
    {
        await Clients.Group($"game-{gameId}").SendAsync("SpaceshipUpdated", spaceship);
    }

    public async Task SendSpaceshipCreated(int gameId, SpaceshipDto spaceship)
    {
        await Clients.Group($"game-{gameId}").SendAsync("SpaceshipCreated", spaceship);
    }

    public async Task SendSpaceshipDestroyed(int gameId, int spaceshipId)
    {
        await Clients.Group($"game-{gameId}").SendAsync("SpaceshipDestroyed", spaceshipId);
    }

    public async Task SendShipyardUpdate(int gameId, ShipyardDto shipyard)
    {
        await Clients.Group($"game-{gameId}").SendAsync("ShipyardUpdated", shipyard);
    }
}

