namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Represents a combat encounter between two or more ships
/// </summary>
public class Battle
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int SystemId { get; set; }
    public BattleState State { get; set; } = BattleState.InProgress;
    
    // Location of battle
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    
    // Battle timing
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int RoundsElapsed { get; set; } = 0;
    
    // Battle result
    public string? WinnerPlayerId { get; set; }
    public BattleEndReason? EndReason { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Game Game { get; set; } = null!;
    public virtual StarSystem System { get; set; } = null!;
    public virtual AppUser? WinnerPlayer { get; set; }
    public virtual ICollection<BattleParticipant> Participants { get; set; } = new List<BattleParticipant>();
    public virtual ICollection<BattleEvent> Events { get; set; } = new List<BattleEvent>();
}

public enum BattleState
{
    InProgress,
    Completed,
    Fled
}

public enum BattleEndReason
{
    AllEnemiesDestroyed,
    OneSideFled,
    Timeout
}
