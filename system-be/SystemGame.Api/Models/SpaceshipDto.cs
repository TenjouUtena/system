namespace SystemGame.Api.Models;

public class SpaceshipDto
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    
    // Construction
    public int? ShipyardId { get; set; }
    public double ConstructionProgress { get; set; }
    public int ConstructionTimeSeconds { get; set; }
    public DateTime? ConstructionStartTime { get; set; }
    public DateTime? ConstructionCompletedTime { get; set; }
    
    // Location
    public int CurrentSystemId { get; set; }
    public string? CurrentSystemName { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    
    // Movement
    public double Speed { get; set; }
    public int? DestinationSystemId { get; set; }
    public string? DestinationSystemName { get; set; }
    public double? DestinationX { get; set; }
    public double? DestinationY { get; set; }
    public DateTime? MovementStartTime { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    
    // Cargo
    public int? CargoIron { get; set; }
    public int? CargoCopper { get; set; }
    public int? CargoFuel { get; set; }
    public int? CargoSoil { get; set; }
    public int CargoCapacity { get; set; }
    
    // Combat
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
