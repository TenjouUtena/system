namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Represents a spaceship that can travel between systems and planets
/// </summary>
public class Spaceship
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ShipType Type { get; set; }
    public ShipState State { get; set; } = ShipState.UnderConstruction;
    
    // Construction
    public int? ShipyardId { get; set; }  // Where it was/is being built
    public double ConstructionProgress { get; set; } = 0.0;  // 0-100%
    public int ConstructionTimeSeconds { get; set; } = 300;  // Time to build
    public DateTime? ConstructionStartTime { get; set; }
    public DateTime? ConstructionCompletedTime { get; set; }
    
    // Location in space
    public int CurrentSystemId { get; set; }
    public double PositionX { get; set; }  // System-relative coordinates
    public double PositionY { get; set; }
    
    // Movement
    public double Speed { get; set; } = 10.0;  // Units per second
    public int? DestinationSystemId { get; set; }  // If traveling via wormhole
    public double? DestinationX { get; set; }  // Target coordinates
    public double? DestinationY { get; set; }
    public DateTime? MovementStartTime { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    
    // Cargo (for freighters)
    public int? CargoIron { get; set; }
    public int? CargoCopper { get; set; }
    public int? CargoFuel { get; set; }
    public int? CargoSoil { get; set; }
    public int CargoCapacity { get; set; } = 0;  // Max cargo
    
    // Combat stats (for future combat system)
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Attack { get; set; } = 0;
    public int Defense { get; set; } = 0;
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual AppUser Player { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
    public virtual StarSystem CurrentSystem { get; set; } = null!;
    public virtual StarSystem? DestinationSystem { get; set; }
    public virtual Shipyard? Shipyard { get; set; }
}
