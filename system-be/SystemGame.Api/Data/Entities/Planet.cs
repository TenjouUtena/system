namespace SystemGame.Api.Data.Entities;

public class Planet
{
    public int Id { get; set; }
    public int SystemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; } // 1-10, determines grid size (20 * size)
    public PlanetType Type { get; set; }
    
    // Navigation properties
    public virtual StarSystem StarSystem { get; set; } = null!;
    public virtual PlanetGrid? Grid { get; set; }
    public virtual ICollection<Builder> Builders { get; set; } = new List<Builder>();
}

public enum PlanetType
{
    Terrestrial = 1,
    GasGiant = 2,
    Ice = 3,
    Desert = 4
}

