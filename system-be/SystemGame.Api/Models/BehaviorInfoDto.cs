namespace SystemGame.Api.Models;

public class BehaviorInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportedAgentTypes { get; set; } = new();
    public string? ConfigSchema { get; set; } // JSON Schema for validation (future enhancement)
}
