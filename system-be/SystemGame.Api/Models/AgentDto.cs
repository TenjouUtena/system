namespace SystemGame.Api.Models;

public class AgentDto
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? CurrentBehaviorName { get; set; }
    public string? BehaviorConfig { get; set; }
    public DateTime LastExecutionTime { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Location
    public int? CurrentSystemId { get; set; }
    public string? CurrentSystemName { get; set; }
    public int? CurrentPlanetId { get; set; }
    public string? CurrentPlanetName { get; set; }
    
    // Entity references
    public int? BuilderId { get; set; }
    public string? BuilderName { get; set; }
}
