namespace SystemGame.Api.Data.Entities;

public class PlanetGrid
{
    public int Id { get; set; }
    public int PlanetId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Planet Planet { get; set; } = null!;
    public virtual ICollection<GridSquare> Squares { get; set; } = new List<GridSquare>();
}

