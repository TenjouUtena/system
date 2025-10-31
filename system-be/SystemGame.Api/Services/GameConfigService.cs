using Microsoft.Extensions.Options;
using SystemGame.Api.Models;

namespace SystemGame.Api.Services;

/// <summary>
/// Service for managing game configuration and balance
/// </summary>
public class GameConfigService
{
    private readonly ILogger<GameConfigService> _logger;
    private GameConfig _config;

    public GameConfigService(
        ILogger<GameConfigService> logger,
        IOptions<GameConfig>? options = null)
    {
        _logger = logger;
        _config = options?.Value ?? new GameConfig();
    }

    /// <summary>
    /// Get current game configuration
    /// </summary>
    public GameConfig GetConfig()
    {
        return _config;
    }

    /// <summary>
    /// Update game configuration
    /// </summary>
    public void UpdateConfig(GameConfig config)
    {
        _config = config;
        _logger.LogInformation("Game configuration updated");
    }

    /// <summary>
    /// Load difficulty preset
    /// </summary>
    public void LoadPreset(string difficulty)
    {
        _config = GameConfigPresets.GetPreset(difficulty);
        _logger.LogInformation("Loaded {Difficulty} difficulty preset", difficulty);
    }

    /// <summary>
    /// Get ship stats by type
    /// </summary>
    public ShipStats GetShipStats(string shipType)
    {
        if (_config.ShipStatsByType.TryGetValue(shipType, out var stats))
        {
            return stats;
        }

        _logger.LogWarning("Ship type {ShipType} not found in config, using default stats", shipType);
        return new ShipStats
        {
            MaxHealth = 100,
            Attack = 20,
            Defense = 10,
            Speed = 10.0,
            CargoCapacity = 50,
            IronCost = 100,
            CopperCost = 50,
            FuelCost = 75
        };
    }

    /// <summary>
    /// Get ship construction time by type
    /// </summary>
    public int GetShipConstructionTime(string shipType)
    {
        if (_config.ShipConstructionTimes.TryGetValue(shipType, out var time))
        {
            return time;
        }

        _logger.LogWarning("Ship type {ShipType} not found in config, using default time", shipType);
        return 300; // 5 minutes default
    }

    /// <summary>
    /// Check if player can afford ship
    /// </summary>
    public bool CanAffordShip(string shipType, int iron, int copper, int fuel)
    {
        var stats = GetShipStats(shipType);
        return iron >= stats.IronCost &&
               copper >= stats.CopperCost &&
               fuel >= stats.FuelCost;
    }

    /// <summary>
    /// Get resource costs for ship type
    /// </summary>
    public (int Iron, int Copper, int Fuel) GetShipCosts(string shipType)
    {
        var stats = GetShipStats(shipType);
        return (stats.IronCost, stats.CopperCost, stats.FuelCost);
    }

    /// <summary>
    /// Calculate production amount for current tick
    /// </summary>
    public double CalculateProductionAmount(int buildingLevel = 1)
    {
        return _config.BaseProductionPerTick * _config.ProductionMultiplier * buildingLevel;
    }

    /// <summary>
    /// Calculate damage for combat
    /// </summary>
    public int CalculateDamage(int attack, int defense, Random random)
    {
        // Base damage with randomness
        var baseDamage = attack * (
            _config.DamageRandomnessMin + 
            random.NextDouble() * (_config.DamageRandomnessMax - _config.DamageRandomnessMin)
        );

        // Defense reduces damage
        var damageReduction = defense * _config.DefenseEfficiency;
        var finalDamage = Math.Max(1, baseDamage - damageReduction);

        return (int)finalDamage;
    }

    /// <summary>
    /// Calculate flee chance based on health percentage
    /// </summary>
    public double CalculateFleeChance(int currentHealth, int maxHealth)
    {
        var healthPercent = (double)currentHealth / maxHealth;
        return _config.FleeChanceBase * (1.0 - healthPercent);
    }

    /// <summary>
    /// Get NPC stats scaled by difficulty level
    /// </summary>
    public (int MaxHealth, int Attack, int Defense) GetNpcStats(int baseHealth, int baseAttack, int baseDefense, int difficultyLevel)
    {
        var multiplier = 1.0 + (difficultyLevel * _config.NpcDifficultyMultiplier);
        return (
            (int)(baseHealth * multiplier),
            (int)(baseAttack * multiplier),
            (int)(baseDefense * multiplier)
        );
    }

    /// <summary>
    /// Export configuration as JSON
    /// </summary>
    public string ExportConfig()
    {
        return System.Text.Json.JsonSerializer.Serialize(_config, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Import configuration from JSON
    /// </summary>
    public bool TryImportConfig(string json)
    {
        try
        {
            var config = System.Text.Json.JsonSerializer.Deserialize<GameConfig>(json);
            if (config != null)
            {
                _config = config;
                _logger.LogInformation("Configuration imported successfully");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import configuration");
        }
        return false;
    }
}
