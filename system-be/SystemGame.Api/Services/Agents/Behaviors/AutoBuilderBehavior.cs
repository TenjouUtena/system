using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents.Behaviors;

public class AutoBuilderBehavior : IAgentBehavior
{
    public string Name => "AutoBuilder";
    public string Description => "Automatically assigns builders to construct queued buildings";
    public AgentType[] SupportedAgentTypes => new[] { AgentType.Builder };

    public async Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context)
    {
        try
        {
            // Parse configuration
            var config = ParseConfig(agent.BehaviorConfig);
            
            // Get the builder entity
            if (!agent.BuilderId.HasValue)
            {
                return BehaviorResult.ErrorResult("Agent does not have a builder assigned");
            }

            var builder = await context.DbContext.Builders
                .Include(b => b.Planet)
                .FirstOrDefaultAsync(b => b.Id == agent.BuilderId.Value, context.CancellationToken);

            if (builder == null)
            {
                return BehaviorResult.ErrorResult("Builder not found");
            }

            // Check if builder is already assigned
            if (!builder.IsAvailable)
            {
                // Builder is busy, wait a bit
                return BehaviorResult.SuccessResult("Builder is currently busy", AgentState.Active);
            }

            // Find incomplete buildings on configured planets
            var query = context.DbContext.Buildings
                .Include(b => b.GridSquare)
                    .ThenInclude(g => g.PlanetGrid)
                .Where(b => !b.IsComplete && 
                           b.PlayerId == agent.PlayerId &&
                           b.AssignedBuilding == null);

            // Filter by planet IDs if configured
            if (config.PlanetIds != null && config.PlanetIds.Any())
            {
                query = query.Where(b => config.PlanetIds.Contains(b.GridSquare.PlanetGrid.PlanetId));
            }

            // Prioritize by building type if configured
            var buildings = await query.ToListAsync(context.CancellationToken);
            
            Building? targetBuilding = null;
            if (config.PriorityBuildingTypes != null && config.PriorityBuildingTypes.Any())
            {
                // First try to find a priority building
                targetBuilding = buildings.FirstOrDefault(b => 
                    config.PriorityBuildingTypes.Contains(b.Type.ToString()));
            }
            
            // If no priority building found, just take the first one
            targetBuilding ??= buildings.FirstOrDefault();

            if (targetBuilding == null)
            {
                // No buildings to construct
                return BehaviorResult.SuccessResult("No buildings awaiting construction", AgentState.Idle);
            }

            // Check max concurrent buildings limit
            if (config.MaxConcurrentBuildings > 0)
            {
                var busyBuildersCount = await context.DbContext.Builders
                    .Where(b => b.PlayerId == agent.PlayerId && !b.IsAvailable)
                    .CountAsync(context.CancellationToken);

                if (busyBuildersCount >= config.MaxConcurrentBuildings)
                {
                    return BehaviorResult.SuccessResult(
                        $"Max concurrent buildings limit reached ({config.MaxConcurrentBuildings})", 
                        AgentState.Active);
                }
            }

            // Assign builder to building
            builder.AssignedBuildingId = targetBuilding.Id;
            builder.IsAvailable = false;
            targetBuilding.ConstructionStartTime = DateTime.UtcNow;

            await context.DbContext.SaveChangesAsync(context.CancellationToken);

            return BehaviorResult.SuccessResult(
                $"Assigned builder to construct {targetBuilding.Type} at building {targetBuilding.Id}",
                AgentState.Active);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error in AutoBuilderBehavior for agent {AgentId}", agent.Id);
            return BehaviorResult.ErrorResult($"Error executing behavior: {ex.Message}");
        }
    }

    public async Task<bool> CanExecuteAsync(Agent agent, BehaviorContext context)
    {
        // Can execute if agent has a builder assigned
        if (!agent.BuilderId.HasValue)
            return false;

        var builder = await context.DbContext.Builders
            .FirstOrDefaultAsync(b => b.Id == agent.BuilderId.Value, context.CancellationToken);
        
        return builder != null;
    }

    public Task ValidateConfigAsync(string? config)
    {
        if (string.IsNullOrWhiteSpace(config))
        {
            return Task.CompletedTask; // Config is optional
        }

        try
        {
            JsonSerializer.Deserialize<AutoBuilderConfig>(config);
            return Task.CompletedTask;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid configuration: {ex.Message}", ex);
        }
    }

    private AutoBuilderConfig ParseConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new AutoBuilderConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<AutoBuilderConfig>(configJson) ?? new AutoBuilderConfig();
        }
        catch
        {
            return new AutoBuilderConfig();
        }
    }

    private class AutoBuilderConfig
    {
        public List<int>? PlanetIds { get; set; }
        public List<string>? PriorityBuildingTypes { get; set; }
        public int MaxConcurrentBuildings { get; set; } = 0; // 0 = unlimited
    }
}
