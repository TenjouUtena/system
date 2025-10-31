namespace SystemGame.Api.Data.Entities;

/// <summary>
/// A shipyard that constructs spaceships at a space station
/// </summary>
public class Shipyard
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public int SpaceStationId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Construction queue
    public int MaxConcurrentBuilds { get; set; } = 1;  // How many ships can build at once
    public bool IsActive { get; set; } = true;
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual AppUser Player { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
    public virtual SpaceStation SpaceStation { get; set; } = null!;
    public virtual ICollection<Spaceship> Spaceships { get; set; } = new List<Spaceship>();
}
