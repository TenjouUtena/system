using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents.Behaviors;

public class ProductionMonitorBehavior : IAgentBehavior
{
    public string Name => "ProductionMonitor";
    public string Description => "Monitors resource production and sends alerts";
    public AgentType[] SupportedAgentTypes => new[] { AgentType.Custom };

    public async Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context)
    {
        try
        {
            var config = ParseConfig(agent.BehaviorConfig);
            
            // Check if enough time has passed since last check
            if (config.CheckInterval > 0)
            {
                var timeSinceLastExecution = DateTime.UtcNow - agent.LastExecutionTime;
                if (timeSinceLastExecution.TotalSeconds < config.CheckInterval)
                {
                    var delay = TimeSpan.FromSeconds(config.CheckInterval - timeSinceLastExecution.TotalSeconds);
                    var result = BehaviorResult.SuccessResult($"Next check in {delay.TotalSeconds:F0}s", AgentState.Active);
                    result.NextExecutionDelay = delay;
                    return result;
                }
            }

            // Get all player's space stations
            var stations = await context.DbContext.SpaceStations
                .Where(s => s.PlayerId == agent.PlayerId)
                .ToListAsync(context.CancellationToken);

            var alerts = new List<string>();

            // Check each monitored resource
            if (config.MonitoredResources != null)
            {
                foreach (var station in stations)
                {
                    foreach (var resource in config.MonitoredResources)
                    {
                        var resourceLower = resource.ToLower();
                        double amount = resourceLower switch
                        {
                            "iron" => station.IronAmount,
                            "copper" => station.CopperAmount,
                            "fuel" => station.FuelAmount,
                            "soil" => station.SoilAmount,
                            _ => 0
                        };

                        // Check if below threshold
                        if (config.AlertThresholds != null && 
                            config.AlertThresholds.TryGetValue(resource, out var threshold))
                        {
                            if (amount < threshold)
                            {
                                var alert = $"Station {station.Name}: {resource} low ({amount:F1} < {threshold})";
                                alerts.Add(alert);
                                context.Logger.LogWarning(alert);
                            }
                        }
                    }
                }
            }

            // Store alert data in shared data for potential UI display
            if (alerts.Any())
            {
                context.SharedData[$"agent_{agent.Id}_alerts"] = alerts;
                
                var result = BehaviorResult.SuccessResult(
                    $"Found {alerts.Count} resource alert(s)",
                    AgentState.Active);
                result.LogData = JsonSerializer.Serialize(new { Alerts = alerts });
                return result;
            }

            return BehaviorResult.SuccessResult("All monitored resources above thresholds", AgentState.Active);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error in ProductionMonitorBehavior for agent {AgentId}", agent.Id);
            return BehaviorResult.ErrorResult($"Error executing behavior: {ex.Message}");
        }
    }

    public Task<bool> CanExecuteAsync(Agent agent, BehaviorContext context)
    {
        return Task.FromResult(true);
    }

    public Task ValidateConfigAsync(string? config)
    {
        if (string.IsNullOrWhiteSpace(config))
        {
            return Task.CompletedTask; // Config is optional
        }

        try
        {
            JsonSerializer.Deserialize<ProductionMonitorConfig>(config);
            return Task.CompletedTask;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid configuration: {ex.Message}", ex);
        }
    }

    private ProductionMonitorConfig ParseConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new ProductionMonitorConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<ProductionMonitorConfig>(configJson) ?? new ProductionMonitorConfig();
        }
        catch
        {
            return new ProductionMonitorConfig();
        }
    }

    private class ProductionMonitorConfig
    {
        public List<string>? MonitoredResources { get; set; }
        public Dictionary<string, double>? AlertThresholds { get; set; }
        public int CheckInterval { get; set; } = 60; // seconds
    }
}
