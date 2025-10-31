namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Represents an event that occurred during a battle
/// </summary>
public class BattleEvent
{
    public int Id { get; set; }
    public int BattleId { get; set; }
    public int Round { get; set; }
    public BattleEventType Type { get; set; }
    
    // Event details
    public int? AttackerShipId { get; set; }
    public int? DefenderShipId { get; set; }
    public int? DamageDealt { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Timestamp
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Battle Battle { get; set; } = null!;
    public virtual Spaceship? AttackerShip { get; set; }
    public virtual Spaceship? DefenderShip { get; set; }
}

public enum BattleEventType
{
    BattleStarted,
    Attack,
    ShipDestroyed,
    ShipFled,
    BattleEnded
}
