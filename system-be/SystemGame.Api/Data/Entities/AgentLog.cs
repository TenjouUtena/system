using Microsoft.Extensions.Logging;

namespace SystemGame.Api.Data.Entities;

public class AgentLog
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; } // JSON for structured data
    
    // Navigation
    public virtual Agent Agent { get; set; } = null!;
}
