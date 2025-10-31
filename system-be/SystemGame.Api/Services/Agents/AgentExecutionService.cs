using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;
using SystemGame.Api.Services.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SystemGame.Api.Services.Agents;

public class AgentExecutionService
{
    private readonly ApplicationDbContext _context;
    private readonly AgentBehaviorService _behaviorService;
    private readonly ILogger<AgentExecutionService> _logger;
    private readonly IHubContext<GameHub> _hubContext;

    public AgentExecutionService(
        ApplicationDbContext context,
        AgentBehaviorService behaviorService,
        ILogger<AgentExecutionService> logger,
        IHubContext<GameHub> hubContext)
    {
        _context = context;
        _behaviorService = behaviorService;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task ProcessAgentsAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.Id == gameId && g.IsActive, cancellationToken);

            if (game == null)
            {
                return;
            }

            // Get all active agents for this game
            var agents = await _context.Agents
                .Include(a => a.Player)
                .Include(a => a.Builder)
                .Include(a => a.CurrentSystem)
                .Include(a => a.CurrentPlanet)
                .Where(a => a.GameId == gameId && a.State == AgentState.Active)
                .ToListAsync(cancellationToken);

            var behaviorContext = new BehaviorContext
            {
                DbContext = _context,
                Logger = _logger,
                Game = game,
                SharedData = new Dictionary<string, object>(),
                CancellationToken = cancellationToken
            };

            foreach (var agent in agents)
            {
                try
                {
                    // Check if agent should execute based on last execution time and delay
                    var timeSinceLastExecution = DateTime.UtcNow - agent.LastExecutionTime;
                    // Minimum 5 seconds between executions by default
                    if (timeSinceLastExecution.TotalSeconds < 5)
                    {
                        continue;
                    }

                    // Execute behavior
                    var result = await _behaviorService.ExecuteBehaviorAsync(agent, behaviorContext);

                    // Update agent state
                    agent.State = result.NextState;
                    agent.LastExecutionTime = DateTime.UtcNow;

                    // Create log entry
                    var logEntry = new AgentLog
                    {
                        AgentId = agent.Id,
                        Timestamp = DateTime.UtcNow,
                        Level = result.Success ? LogLevel.Information : LogLevel.Error,
                        Message = result.Message ?? "Behavior executed",
                        Data = result.LogData
                    };
                    _context.AgentLogs.Add(logEntry);

                    await _context.SaveChangesAsync(cancellationToken);

                    // Broadcast agent update via SignalR
                    await BroadcastAgentUpdate(agent);
                    await BroadcastAgentLog(agent.GameId, logEntry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing agent {AgentId}", agent.Id);
                    
                    // Mark agent as error state
                    agent.State = AgentState.Error;
                    agent.LastExecutionTime = DateTime.UtcNow;
                    
                    var errorLog = new AgentLog
                    {
                        AgentId = agent.Id,
                        Timestamp = DateTime.UtcNow,
                        Level = LogLevel.Error,
                        Message = $"Agent execution error: {ex.Message}"
                    };
                    _context.AgentLogs.Add(errorLog);
                    
                    await _context.SaveChangesAsync(cancellationToken);
                    await BroadcastAgentUpdate(agent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing agents for game {GameId}", gameId);
        }
    }

    public async Task<Agent> CreateAgentAsync(CreateAgentRequest request, string playerId)
    {
        // Validate agent type
        if (!Enum.TryParse<AgentType>(request.Type, true, out var agentType))
        {
            throw new ArgumentException($"Invalid agent type: {request.Type}");
        }

        // Validate behavior if specified
        if (!string.IsNullOrWhiteSpace(request.BehaviorName))
        {
            await _behaviorService.ValidateBehaviorConfigAsync(request.BehaviorName, request.BehaviorConfig);
        }

        // Verify game exists and player is a member
        var playerGame = await _context.PlayerGames
            .FirstOrDefaultAsync(pg => pg.GameId == request.GameId && pg.UserId == playerId);

        if (playerGame == null)
        {
            throw new InvalidOperationException("Player is not a member of this game");
        }

        // If builder is specified, verify it exists and belongs to player
        if (request.BuilderId.HasValue)
        {
            var builder = await _context.Builders
                .FirstOrDefaultAsync(b => b.Id == request.BuilderId.Value && b.PlayerId == playerId);

            if (builder == null)
            {
                throw new ArgumentException("Builder not found or does not belong to player");
            }
        }

        var agent = new Agent
        {
            PlayerId = playerId,
            GameId = request.GameId,
            Type = agentType,
            Name = request.Name,
            State = AgentState.Active,
            CurrentBehaviorName = request.BehaviorName,
            BehaviorConfig = request.BehaviorConfig,
            BuilderId = request.BuilderId,
            CreatedAt = DateTime.UtcNow,
            LastExecutionTime = DateTime.UtcNow
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created agent {AgentId} for player {PlayerId}", agent.Id, playerId);

        await BroadcastAgentUpdate(agent);

        return agent;
    }

    public async Task<Agent> UpdateAgentAsync(int agentId, UpdateAgentRequest request, string playerId)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.PlayerId == playerId);

        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to player");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            agent.Name = request.Name;
        }

        // Update behavior if provided
        if (!string.IsNullOrWhiteSpace(request.BehaviorName))
        {
            await _behaviorService.ValidateBehaviorConfigAsync(request.BehaviorName, request.BehaviorConfig);
            agent.CurrentBehaviorName = request.BehaviorName;
            agent.BehaviorConfig = request.BehaviorConfig;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated agent {AgentId}", agentId);

        await BroadcastAgentUpdate(agent);

        return agent;
    }

    public async Task PauseAgentAsync(int agentId, string playerId)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.PlayerId == playerId);

        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to player");
        }

        agent.State = AgentState.Paused;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Paused agent {AgentId}", agentId);

        await BroadcastAgentUpdate(agent);
    }

    public async Task ResumeAgentAsync(int agentId, string playerId)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.PlayerId == playerId);

        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to player");
        }

        agent.State = AgentState.Active;
        agent.LastExecutionTime = DateTime.UtcNow; // Reset timer
        await _context.SaveChangesAsync();

        _logger.LogInformation("Resumed agent {AgentId}", agentId);

        await BroadcastAgentUpdate(agent);
    }

    public async Task DeleteAgentAsync(int agentId, string playerId)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.PlayerId == playerId);

        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to player");
        }

        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted agent {AgentId}", agentId);

        // Broadcast deletion
        await _hubContext.Clients.Group($"game-{agent.GameId}")
            .SendAsync("AgentDeleted", agentId);
    }

    public async Task<List<AgentLog>> GetAgentLogsAsync(int agentId, string playerId, int limit = 100)
    {
        // Verify agent belongs to player
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.PlayerId == playerId);

        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to player");
        }

        return await _context.AgentLogs
            .Where(l => l.AgentId == agentId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    private async Task BroadcastAgentUpdate(Agent agent)
    {
        var dto = MapToDto(agent);
        await _hubContext.Clients.Group($"game-{agent.GameId}")
            .SendAsync("AgentUpdated", dto);
    }

    private async Task BroadcastAgentLog(int gameId, AgentLog log)
    {
        var dto = new AgentLogDto
        {
            Id = log.Id,
            Timestamp = log.Timestamp,
            Level = log.Level.ToString(),
            Message = log.Message,
            Data = log.Data
        };

        await _hubContext.Clients.Group($"game-{gameId}")
            .SendAsync("AgentLogReceived", log.AgentId, dto);
    }

    private AgentDto MapToDto(Agent agent)
    {
        return new AgentDto
        {
            Id = agent.Id,
            PlayerId = agent.PlayerId,
            GameId = agent.GameId,
            Type = agent.Type.ToString(),
            Name = agent.Name,
            State = agent.State.ToString(),
            CurrentBehaviorName = agent.CurrentBehaviorName,
            BehaviorConfig = agent.BehaviorConfig,
            LastExecutionTime = agent.LastExecutionTime,
            CreatedAt = agent.CreatedAt,
            CurrentSystemId = agent.CurrentSystemId,
            CurrentSystemName = agent.CurrentSystem?.Name,
            CurrentPlanetId = agent.CurrentPlanetId,
            CurrentPlanetName = agent.CurrentPlanet?.Name,
            BuilderId = agent.BuilderId,
            BuilderName = agent.Builder?.Name
        };
    }
}
