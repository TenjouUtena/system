using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents;

public class BehaviorResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AgentState NextState { get; set; } = AgentState.Active;
    public string? LogData { get; set; } // JSON for logging
    public TimeSpan? NextExecutionDelay { get; set; } // For throttling
    
    public static BehaviorResult SuccessResult(string? message = null, AgentState? nextState = null)
    {
        return new BehaviorResult
        {
            Success = true,
            Message = message,
            NextState = nextState ?? AgentState.Active
        };
    }
    
    public static BehaviorResult ErrorResult(string message, string? logData = null)
    {
        return new BehaviorResult
        {
            Success = false,
            Message = message,
            NextState = AgentState.Error,
            LogData = logData
        };
    }
    
    public static BehaviorResult IdleResult(TimeSpan delay)
    {
        return new BehaviorResult
        {
            Success = true,
            Message = "Agent idle",
            NextState = AgentState.Idle,
            NextExecutionDelay = delay
        };
    }
}
