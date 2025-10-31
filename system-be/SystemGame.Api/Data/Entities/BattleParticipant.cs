namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Represents a ship's participation in a battle
/// </summary>
public class BattleParticipant
{
    public int Id { get; set; }
    public int BattleId { get; set; }
    public int SpaceshipId { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public bool IsNpc { get; set; } = false;
    
    // Combat stats snapshot
    public int InitialHealth { get; set; }
    public int FinalHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    
    // Battle results
    public int DamageDealt { get; set; } = 0;
    public int DamageTaken { get; set; } = 0;
    public bool Survived { get; set; } = true;
    public bool Fled { get; set; } = false;
    
    // Rewards (for survivors)
    public int ExperienceGained { get; set; } = 0;
    public int? LootIron { get; set; }
    public int? LootCopper { get; set; }
    public int? LootFuel { get; set; }
    
    // Navigation properties
    public virtual Battle Battle { get; set; } = null!;
    public virtual Spaceship Spaceship { get; set; } = null!;
    public virtual AppUser Player { get; set; } = null!;
}
