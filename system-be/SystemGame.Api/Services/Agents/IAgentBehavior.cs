using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents;

public interface IAgentBehavior
{
    string Name { get; }
    string Description { get; }
    AgentType[] SupportedAgentTypes { get; }
    
    Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context);
    Task<bool> CanExecuteAsync(Agent agent, BehaviorContext context);
    Task ValidateConfigAsync(string? config);
}
