namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Represents metadata for NPC-controlled ships
/// </summary>
public class NpcShip
{
    public int Id { get; set; }
    public int SpaceshipId { get; set; }
    public int GameId { get; set; }
    public NpcBehaviorType BehaviorType { get; set; }
    public int DifficultyLevel { get; set; } = 1; // 1-10 scale
    
    // Behavior state
    public double? PatrolTargetX { get; set; }
    public double? PatrolTargetY { get; set; }
    public int? TargetShipId { get; set; } // For aggressive NPCs
    public DateTime LastBehaviorUpdate { get; set; } = DateTime.UtcNow;
    
    // Spawn information
    public DateTime SpawnTime { get; set; } = DateTime.UtcNow;
    public int? SpawnSystemId { get; set; }
    
    // Loot configuration
    public int LootIronMin { get; set; } = 0;
    public int LootIronMax { get; set; } = 0;
    public int LootCopperMin { get; set; } = 0;
    public int LootCopperMax { get; set; } = 0;
    public int LootFuelMin { get; set; } = 0;
    public int LootFuelMax { get; set; } = 0;
    
    // Navigation properties
    public virtual Spaceship Spaceship { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
    public virtual StarSystem? SpawnSystem { get; set; }
    public virtual Spaceship? TargetShip { get; set; }
}

public enum NpcBehaviorType
{
    Patrol,      // Moves randomly within system
    Ambush,      // Waits near high-traffic areas
    Aggressive,  // Actively seeks out player ships
    Passive      // Only defends itself if attacked
}
