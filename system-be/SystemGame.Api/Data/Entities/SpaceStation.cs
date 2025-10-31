namespace SystemGame.Api.Data.Entities;

public class SpaceStation
{
    public int Id { get; set; }
    public int SystemId { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Resource storage
    public double IronAmount { get; set; } = 0;
    public double CopperAmount { get; set; } = 0;
    public double FuelAmount { get; set; } = 0;
    public double SoilAmount { get; set; } = 0;
    
    // Navigation properties
    public virtual StarSystem StarSystem { get; set; } = null!;
    public virtual AppUser Player { get; set; } = null!;
}

