using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents.Behaviors;

public class ResourceFerryBehavior : IAgentBehavior
{
    public string Name => "ResourceFerry";
    public string Description => "Transports resources between space stations";
    public AgentType[] SupportedAgentTypes => new[] { AgentType.ResourceFerry };

    public async Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context)
    {
        try
        {
            // Parse configuration
            var config = ParseConfig(agent.BehaviorConfig);
            
            if (!config.SourceSystemId.HasValue || !config.TargetSystemId.HasValue)
            {
                return BehaviorResult.ErrorResult("Source and target system IDs must be configured");
            }

            // Check if enough time has passed since last execution
            if (config.FerryInterval > 0)
            {
                var timeSinceLastExecution = DateTime.UtcNow - agent.LastExecutionTime;
                if (timeSinceLastExecution.TotalSeconds < config.FerryInterval)
                {
                    var delay = TimeSpan.FromSeconds(config.FerryInterval - timeSinceLastExecution.TotalSeconds);
                    var result = BehaviorResult.SuccessResult($"Waiting {delay.TotalSeconds:F0}s before next ferry", AgentState.Active);
                    result.NextExecutionDelay = delay;
                    return result;
                }
            }

            // Get source station
            var sourceStation = await context.DbContext.SpaceStations
                .FirstOrDefaultAsync(s => 
                    s.SystemId == config.SourceSystemId.Value && 
                    s.PlayerId == agent.PlayerId, 
                    context.CancellationToken);

            if (sourceStation == null)
            {
                return BehaviorResult.ErrorResult("Source space station not found");
            }

            // Get or create target station
            var targetStation = await context.DbContext.SpaceStations
                .FirstOrDefaultAsync(s => 
                    s.SystemId == config.TargetSystemId.Value && 
                    s.PlayerId == agent.PlayerId, 
                    context.CancellationToken);

            if (targetStation == null)
            {
                // Create target station if it doesn't exist
                var targetSystem = await context.DbContext.StarSystems
                    .FirstOrDefaultAsync(s => s.Id == config.TargetSystemId.Value, context.CancellationToken);

                if (targetSystem == null)
                {
                    return BehaviorResult.ErrorResult("Target system not found");
                }

                var player = await context.DbContext.Users.FindAsync(new object[] { agent.PlayerId }, context.CancellationToken);
                
                targetStation = new SpaceStation
                {
                    SystemId = config.TargetSystemId.Value,
                    PlayerId = agent.PlayerId,
                    Name = $"{player?.DisplayName ?? "Player"}'s Station",
                    CreatedAt = DateTime.UtcNow
                };
                context.DbContext.SpaceStations.Add(targetStation);
                await context.DbContext.SaveChangesAsync(context.CancellationToken);
            }

            // Determine resource type and amount to ferry
            var resourceType = config.ResourceType?.ToLower() ?? "iron";
            double sourceAmount = resourceType switch
            {
                "iron" => sourceStation.IronAmount,
                "copper" => sourceStation.CopperAmount,
                "fuel" => sourceStation.FuelAmount,
                "soil" => sourceStation.SoilAmount,
                _ => 0
            };

            // Check if source has minimum amount
            if (sourceAmount < config.MinAmount)
            {
                return BehaviorResult.SuccessResult(
                    $"Source station has insufficient {resourceType} ({sourceAmount:F1} < {config.MinAmount})",
                    AgentState.Idle);
            }

            // Calculate ferry amount - ensure we don't transfer more than available or max
            var ferryAmount = Math.Min(sourceAmount, config.MaxAmount);
            // Only apply minimum if we have at least that much
            if (ferryAmount < config.MinAmount)
            {
                return BehaviorResult.SuccessResult(
                    $"Available amount ({ferryAmount:F1}) is less than minimum ferry amount ({config.MinAmount})",
                    AgentState.Idle);
            }

            // Transfer resources
            switch (resourceType)
            {
                case "iron":
                    sourceStation.IronAmount -= ferryAmount;
                    targetStation.IronAmount += ferryAmount;
                    break;
                case "copper":
                    sourceStation.CopperAmount -= ferryAmount;
                    targetStation.CopperAmount += ferryAmount;
                    break;
                case "fuel":
                    sourceStation.FuelAmount -= ferryAmount;
                    targetStation.FuelAmount += ferryAmount;
                    break;
                case "soil":
                    sourceStation.SoilAmount -= ferryAmount;
                    targetStation.SoilAmount += ferryAmount;
                    break;
            }

            await context.DbContext.SaveChangesAsync(context.CancellationToken);

            return BehaviorResult.SuccessResult(
                $"Ferried {ferryAmount:F1} {resourceType} from system {config.SourceSystemId} to {config.TargetSystemId}",
                AgentState.Active);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error in ResourceFerryBehavior for agent {AgentId}", agent.Id);
            return BehaviorResult.ErrorResult($"Error executing behavior: {ex.Message}");
        }
    }

    public async Task<bool> CanExecuteAsync(Agent agent, BehaviorContext context)
    {
        var config = ParseConfig(agent.BehaviorConfig);
        return config.SourceSystemId.HasValue && config.TargetSystemId.HasValue;
    }

    public Task ValidateConfigAsync(string? config)
    {
        if (string.IsNullOrWhiteSpace(config))
        {
            throw new ArgumentException("Configuration is required for ResourceFerry behavior");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<ResourceFerryConfig>(config);
            if (parsed == null || !parsed.SourceSystemId.HasValue || !parsed.TargetSystemId.HasValue)
            {
                throw new ArgumentException("Source and target system IDs are required");
            }
            return Task.CompletedTask;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid configuration: {ex.Message}", ex);
        }
    }

    private ResourceFerryConfig ParseConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new ResourceFerryConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<ResourceFerryConfig>(configJson) ?? new ResourceFerryConfig();
        }
        catch
        {
            return new ResourceFerryConfig();
        }
    }

    private class ResourceFerryConfig
    {
        public int? SourceSystemId { get; set; }
        public int? TargetSystemId { get; set; }
        public string ResourceType { get; set; } = "Iron";
        public double MinAmount { get; set; } = 100;
        public double MaxAmount { get; set; } = 500;
        public int FerryInterval { get; set; } = 300; // seconds
    }
}
