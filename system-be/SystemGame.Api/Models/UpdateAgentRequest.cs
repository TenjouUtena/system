using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class UpdateAgentRequest
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }
    
    public string? BehaviorName { get; set; }
    
    public string? BehaviorConfig { get; set; } // JSON
}
