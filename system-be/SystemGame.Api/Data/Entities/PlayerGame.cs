namespace SystemGame.Api.Data.Entities;

public class PlayerGame
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
}

