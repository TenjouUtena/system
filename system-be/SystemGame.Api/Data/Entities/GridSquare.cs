namespace SystemGame.Api.Data.Entities;

public class GridSquare
{
    public int Id { get; set; }
    public int PlanetGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    
    // Resources stored as nullable amounts per resource type
    public double? IronAmount { get; set; }
    public double? CopperAmount { get; set; }
    public double? FuelAmount { get; set; }
    public double? SoilAmount { get; set; }
    
    // Navigation properties
    public virtual PlanetGrid PlanetGrid { get; set; } = null!;
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
}

