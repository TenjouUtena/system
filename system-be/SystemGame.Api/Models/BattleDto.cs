namespace SystemGame.Api.Models;

public class BattleDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int SystemId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int RoundsElapsed { get; set; }
    
    public string? WinnerPlayerId { get; set; }
    public string? WinnerPlayerName { get; set; }
    public string? EndReason { get; set; }
    
    public List<BattleParticipantDto> Participants { get; set; } = new();
    public List<BattleEventDto> Events { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}

public class BattleParticipantDto
{
    public int Id { get; set; }
    public int BattleId { get; set; }
    public int SpaceshipId { get; set; }
    public string SpaceshipName { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public bool IsNpc { get; set; }
    
    public int InitialHealth { get; set; }
    public int FinalHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public bool Survived { get; set; }
    public bool Fled { get; set; }
    
    public int ExperienceGained { get; set; }
    public int? LootIron { get; set; }
    public int? LootCopper { get; set; }
    public int? LootFuel { get; set; }
}

public class BattleEventDto
{
    public int Id { get; set; }
    public int BattleId { get; set; }
    public int Round { get; set; }
    public string Type { get; set; } = string.Empty;
    
    public int? AttackerShipId { get; set; }
    public string? AttackerShipName { get; set; }
    public int? DefenderShipId { get; set; }
    public string? DefenderShipName { get; set; }
    public int? DamageDealt { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; }
}

public class BattleSummaryDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int SystemId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int RoundsElapsed { get; set; }
    
    public string? WinnerPlayerName { get; set; }
    public int ParticipantCount { get; set; }
}
