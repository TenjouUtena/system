using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents.Behaviors;

public class IdleBehavior : IAgentBehavior
{
    public string Name => "Idle";
    public string Description => "Default idle behavior when no other behavior is assigned";
    public AgentType[] SupportedAgentTypes => Enum.GetValues<AgentType>();

    public Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context)
    {
        // Just log that we're idle and wait
        return Task.FromResult(BehaviorResult.IdleResult(TimeSpan.FromSeconds(30)));
    }

    public Task<bool> CanExecuteAsync(Agent agent, BehaviorContext context)
    {
        return Task.FromResult(true);
    }

    public Task ValidateConfigAsync(string? config)
    {
        // No config needed for idle behavior
        return Task.CompletedTask;
    }
}
