using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class CreateAgentRequest
{
    [Required]
    public int GameId { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty; // AgentType as string
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    public string? BehaviorName { get; set; }
    
    public string? BehaviorConfig { get; set; } // JSON
    
    public int? BuilderId { get; set; }
}
