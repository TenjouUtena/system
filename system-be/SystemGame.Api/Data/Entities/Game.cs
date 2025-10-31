namespace SystemGame.Api.Data.Entities;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CreatorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxPlayers { get; set; } = 50;
    
    // Navigation properties
    public virtual AppUser? Creator { get; set; }
    public virtual ICollection<PlayerGame> Players { get; set; } = new List<PlayerGame>();
    public virtual Galaxy? Galaxy { get; set; }
}

