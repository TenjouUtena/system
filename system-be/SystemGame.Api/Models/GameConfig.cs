namespace SystemGame.Api.Models;

/// <summary>
/// Centralized game balance configuration
/// All game mechanics parameters in one place for easy tuning
/// </summary>
public class GameConfig
{
    // ===== RESOURCE PRODUCTION =====
    
    /// <summary>
    /// Base resource production per simulation tick (5 seconds)
    /// </summary>
    public double BaseProductionPerTick { get; set; } = 0.1;
    
    /// <summary>
    /// Resource production multiplier for different building levels (future use)
    /// </summary>
    public double ProductionMultiplier { get; set; } = 1.0;
    
    // ===== CONSTRUCTION TIMES =====
    
    /// <summary>
    /// Building construction time in seconds
    /// </summary>
    public int BuildingConstructionSeconds { get; set; } = 300; // 5 minutes
    
    /// <summary>
    /// Ship construction time multipliers by type (base seconds)
    /// </summary>
    public Dictionary<string, int> ShipConstructionTimes { get; set; } = new()
    {
        { "Scout", 120 },      // 2 minutes
        { "Colony", 300 },     // 5 minutes  
        { "Freighter", 240 },  // 4 minutes
        { "Destroyer", 600 },  // 10 minutes
        { "Cruiser", 900 },    // 15 minutes
        { "Carrier", 1200 },   // 20 minutes
        { "Capital", 1800 }    // 30 minutes
    };
    
    // ===== SHIP STATS =====
    
    public Dictionary<string, ShipStats> ShipStatsByType { get; set; } = new()
    {
        { "Scout", new ShipStats { MaxHealth = 50, Attack = 10, Defense = 5, Speed = 15.0, CargoCapacity = 10, IronCost = 20, CopperCost = 10, FuelCost = 15 } },
        { "Colony", new ShipStats { MaxHealth = 100, Attack = 5, Defense = 10, Speed = 8.0, CargoCapacity = 100, IronCost = 100, CopperCost = 50, FuelCost = 75 } },
        { "Freighter", new ShipStats { MaxHealth = 150, Attack = 5, Defense = 15, Speed = 10.0, CargoCapacity = 500, IronCost = 80, CopperCost = 40, FuelCost = 60 } },
        { "Destroyer", new ShipStats { MaxHealth = 300, Attack = 60, Defense = 30, Speed = 12.0, CargoCapacity = 50, IronCost = 200, CopperCost = 100, FuelCost = 150 } },
        { "Cruiser", new ShipStats { MaxHealth = 500, Attack = 100, Defense = 60, Speed = 10.0, CargoCapacity = 100, IronCost = 400, CopperCost = 200, FuelCost = 300 } },
        { "Carrier", new ShipStats { MaxHealth = 700, Attack = 80, Defense = 100, Speed = 8.0, CargoCapacity = 200, IronCost = 600, CopperCost = 300, FuelCost = 450 } },
        { "Capital", new ShipStats { MaxHealth = 1000, Attack = 150, Defense = 120, Speed = 6.0, CargoCapacity = 150, IronCost = 1000, CopperCost = 500, FuelCost = 750 } }
    };
    
    // ===== COMBAT BALANCE =====
    
    /// <summary>
    /// Detection range for automatic combat (units)
    /// </summary>
    public double CombatDetectionRange { get; set; } = 50.0;
    
    /// <summary>
    /// Maximum combat rounds before timeout
    /// </summary>
    public int MaxCombatRounds { get; set; } = 20;
    
    /// <summary>
    /// Base flee chance per round (modified by health)
    /// </summary>
    public double FleeChanceBase { get; set; } = 0.15; // 15%
    
    /// <summary>
    /// Damage calculation: Attack * (min-max random)
    /// </summary>
    public double DamageRandomnessMin { get; set; } = 0.8; // 80%
    public double DamageRandomnessMax { get; set; } = 1.2; // 120%
    
    /// <summary>
    /// Defense damage reduction efficiency
    /// </summary>
    public double DefenseEfficiency { get; set; } = 0.5; // 50%
    
    // ===== NPC SPAWNING =====
    
    /// <summary>
    /// Minimum NPCs per game
    /// </summary>
    public int MinNpcsPerGame { get; set; } = 3;
    
    /// <summary>
    /// Maximum NPCs per game
    /// </summary>
    public int MaxNpcsPerGame { get; set; } = 10;
    
    /// <summary>
    /// NPC difficulty scaling multiplier per level
    /// </summary>
    public double NpcDifficultyMultiplier { get; set; } = 0.2; // 20% per level
    
    // ===== GALAXY GENERATION =====
    
    /// <summary>
    /// Default number of star systems for small galaxy
    /// </summary>
    public int SmallGalaxySystemCount { get; set; } = 10;
    
    /// <summary>
    /// Default number of star systems for medium galaxy
    /// </summary>
    public int MediumGalaxySystemCount { get; set; } = 20;
    
    /// <summary>
    /// Default number of star systems for large galaxy
    /// </summary>
    public int LargeGalaxySystemCount { get; set; } = 30;
    
    /// <summary>
    /// Minimum distance between star systems
    /// </summary>
    public int MinSystemDistance { get; set; } = 100;
    
    /// <summary>
    /// Planets per system (min-max)
    /// </summary>
    public int MinPlanetsPerSystem { get; set; } = 1;
    public int MaxPlanetsPerSystem { get; set; } = 4;
    
    /// <summary>
    /// Wormholes per system (min-max)
    /// </summary>
    public int MinWormholesPerSystem { get; set; } = 1;
    public int MaxWormholesPerSystem { get; set; } = 4;
    
    // ===== SIMULATION =====
    
    /// <summary>
    /// Simulation tick interval in seconds
    /// </summary>
    public int SimulationTickSeconds { get; set; } = 5;
    
    /// <summary>
    /// How often to process game ticks in milliseconds
    /// </summary>
    public int GameTickIntervalMs { get; set; } = 5000;
    
    // ===== BUILDERS =====
    
    /// <summary>
    /// Starting number of builders per colonized planet
    /// </summary>
    public int StartingBuildersPerPlanet { get; set; } = 3;
    
    // ===== STARTING RESOURCES =====
    
    /// <summary>
    /// Starting resources for new players
    /// </summary>
    public int StartingIron { get; set; } = 1000;
    public int StartingCopper { get; set; } = 500;
    public int StartingFuel { get; set; } = 500;
    public int StartingSoil { get; set; } = 200;
}

/// <summary>
/// Ship stats for balance configuration
/// </summary>
public class ShipStats
{
    public int MaxHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public double Speed { get; set; }
    public int CargoCapacity { get; set; }
    
    // Resource costs
    public int IronCost { get; set; }
    public int CopperCost { get; set; }
    public int FuelCost { get; set; }
}

/// <summary>
/// Difficulty presets for different game modes
/// </summary>
public static class GameConfigPresets
{
    public static GameConfig Easy => new GameConfig
    {
        BaseProductionPerTick = 0.15,
        BuildingConstructionSeconds = 180, // 3 minutes
        FleeChanceBase = 0.25,
        MinNpcsPerGame = 2,
        MaxNpcsPerGame = 5,
        StartingIron = 2000,
        StartingCopper = 1000,
        StartingFuel = 1000,
        StartingSoil = 500
    };
    
    public static GameConfig Normal => new GameConfig
    {
        // Uses default values
    };
    
    public static GameConfig Hard => new GameConfig
    {
        BaseProductionPerTick = 0.05,
        BuildingConstructionSeconds = 600, // 10 minutes
        FleeChanceBase = 0.10,
        MinNpcsPerGame = 5,
        MaxNpcsPerGame = 15,
        NpcDifficultyMultiplier = 0.3, // 30% per level
        StartingIron = 500,
        StartingCopper = 250,
        StartingFuel = 250,
        StartingSoil = 100
    };
    
    public static GameConfig GetPreset(string difficulty)
    {
        return difficulty?.ToLower() switch
        {
            "easy" => Easy,
            "hard" => Hard,
            _ => Normal
        };
    }
}
