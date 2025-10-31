namespace SystemGame.Api.Data.Entities;

public class StarSystem
{
    public int Id { get; set; }
    public int GalaxyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    
    // Navigation properties
    public virtual Galaxy Galaxy { get; set; } = null!;
    public virtual ICollection<Wormhole> WormholeAConnections { get; set; } = new List<Wormhole>();
    public virtual ICollection<Wormhole> WormholeBConnections { get; set; } = new List<Wormhole>();
    public virtual ICollection<Planet> Planets { get; set; } = new List<Planet>();
    public virtual ICollection<SpaceStation> SpaceStations { get; set; } = new List<SpaceStation>();
}

