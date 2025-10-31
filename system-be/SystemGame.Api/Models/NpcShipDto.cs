namespace SystemGame.Api.Models;

public class NpcShipDto
{
    public int Id { get; set; }
    public int SpaceshipId { get; set; }
    public int GameId { get; set; }
    public string BehaviorType { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    
    public double? PatrolTargetX { get; set; }
    public double? PatrolTargetY { get; set; }
    public int? TargetShipId { get; set; }
    public string? TargetShipName { get; set; }
    
    public DateTime SpawnTime { get; set; }
    public int? SpawnSystemId { get; set; }
    public string? SpawnSystemName { get; set; }
    
    public int LootIronMin { get; set; }
    public int LootIronMax { get; set; }
    public int LootCopperMin { get; set; }
    public int LootCopperMax { get; set; }
    public int LootFuelMin { get; set; }
    public int LootFuelMax { get; set; }
    
    // Include spaceship data
    public SpaceshipDto? Spaceship { get; set; }
}

public class SpawnNpcRequest
{
    public int? SystemId { get; set; }
    public int? DifficultyLevel { get; set; }
}
