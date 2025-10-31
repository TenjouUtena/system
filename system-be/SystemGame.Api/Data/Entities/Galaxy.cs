namespace SystemGame.Api.Data.Entities;

public class Galaxy
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SystemCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Game Game { get; set; } = null!;
    public virtual ICollection<StarSystem> Systems { get; set; } = new List<StarSystem>();
}

