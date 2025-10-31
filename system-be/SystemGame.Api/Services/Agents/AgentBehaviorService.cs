using SystemGame.Api.Data.Entities;
using SystemGame.Api.Services.Agents.Behaviors;

namespace SystemGame.Api.Services.Agents;

public class AgentBehaviorService
{
    private readonly Dictionary<string, IAgentBehavior> _behaviors;
    private readonly ILogger<AgentBehaviorService> _logger;

    public AgentBehaviorService(ILogger<AgentBehaviorService> logger)
    {
        _logger = logger;
        _behaviors = new Dictionary<string, IAgentBehavior>(StringComparer.OrdinalIgnoreCase);
        
        // Register built-in behaviors
        RegisterBehavior(new IdleBehavior());
        RegisterBehavior(new AutoBuilderBehavior());
        RegisterBehavior(new ResourceFerryBehavior());
        RegisterBehavior(new ProductionMonitorBehavior());
    }

    public void RegisterBehavior(IAgentBehavior behavior)
    {
        if (_behaviors.ContainsKey(behavior.Name))
        {
            _logger.LogWarning("Behavior {BehaviorName} is already registered, overwriting", behavior.Name);
        }
        
        _behaviors[behavior.Name] = behavior;
        _logger.LogInformation("Registered behavior: {BehaviorName}", behavior.Name);
    }

    public IAgentBehavior? GetBehavior(string name)
    {
        return _behaviors.GetValueOrDefault(name);
    }

    public IEnumerable<IAgentBehavior> GetBehaviorsForType(AgentType type)
    {
        return _behaviors.Values.Where(b => b.SupportedAgentTypes.Contains(type));
    }

    public IEnumerable<IAgentBehavior> GetAllBehaviors()
    {
        return _behaviors.Values;
    }

    public async Task<BehaviorResult> ExecuteBehaviorAsync(Agent agent, BehaviorContext context)
    {
        var behaviorName = agent.CurrentBehaviorName ?? "Idle";
        var behavior = GetBehavior(behaviorName);

        if (behavior == null)
        {
            _logger.LogWarning("Behavior {BehaviorName} not found for agent {AgentId}, using Idle", 
                behaviorName, agent.Id);
            behavior = GetBehavior("Idle")!;
        }

        try
        {
            // Check if behavior can execute
            if (!await behavior.CanExecuteAsync(agent, context))
            {
                return BehaviorResult.ErrorResult($"Behavior {behaviorName} cannot execute for this agent");
            }

            // Execute behavior
            var result = await behavior.ExecuteAsync(agent, context);
            
            _logger.LogInformation(
                "Agent {AgentId} executed behavior {BehaviorName}: {Success} - {Message}",
                agent.Id, behaviorName, result.Success, result.Message);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing behavior {BehaviorName} for agent {AgentId}", 
                behaviorName, agent.Id);
            return BehaviorResult.ErrorResult($"Behavior execution failed: {ex.Message}");
        }
    }

    public async Task ValidateBehaviorConfigAsync(string behaviorName, string? config)
    {
        var behavior = GetBehavior(behaviorName);
        if (behavior == null)
        {
            throw new ArgumentException($"Behavior {behaviorName} not found");
        }

        await behavior.ValidateConfigAsync(config);
    }
}
