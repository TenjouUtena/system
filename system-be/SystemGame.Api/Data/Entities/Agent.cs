namespace SystemGame.Api.Data.Entities;

public class Agent
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public AgentType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public AgentState State { get; set; } = AgentState.Idle;
    public string? CurrentBehaviorName { get; set; }
    public string? BehaviorConfig { get; set; } // JSON configuration
    public DateTime LastExecutionTime { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Location tracking (nullable - not all agents have locations)
    public int? CurrentSystemId { get; set; }
    public int? CurrentPlanetId { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    
    // Entity references (polymorphic)
    public int? BuilderId { get; set; }
    public int? SpaceshipId { get; set; } // Future: Phase 7
    
    // Navigation properties
    public virtual AppUser Player { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
    public virtual StarSystem? CurrentSystem { get; set; }
    public virtual Planet? CurrentPlanet { get; set; }
    public virtual Builder? Builder { get; set; }
    public virtual ICollection<AgentLog> Logs { get; set; } = new List<AgentLog>();
}
