using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services;

/// <summary>
/// Service for spawning and managing NPC ships (pirates, etc.)
/// </summary>
public class NpcSpawnService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NpcSpawnService> _logger;
    private static readonly Random _random = new Random();
    
    // Spawn configuration
    private const int MIN_NPCS_PER_GAME = 3;
    private const int MAX_NPCS_PER_GAME = 10;
    private const double SPAWN_CHECK_INTERVAL_MINUTES = 30; // Check every 30 minutes

    public NpcSpawnService(
        ApplicationDbContext context,
        ILogger<NpcSpawnService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Check and spawn NPCs if needed for a game
    /// </summary>
    public async Task ProcessNpcSpawningAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Count existing NPCs
            var existingNpcCount = await _context.NpcShips
                .Include(n => n.Spaceship)
                .Where(n => n.GameId == gameId && n.Spaceship.Health > 0)
                .CountAsync(cancellationToken);

            // Spawn new NPCs if below minimum
            if (existingNpcCount < MIN_NPCS_PER_GAME)
            {
                var npcsToSpawn = MIN_NPCS_PER_GAME - existingNpcCount;
                for (int i = 0; i < npcsToSpawn; i++)
                {
                    await SpawnNpcAsync(gameId, cancellationToken: cancellationToken);
                }
            }
            // Randomly spawn additional NPCs up to max
            else if (existingNpcCount < MAX_NPCS_PER_GAME && _random.NextDouble() < 0.1) // 10% chance per check
            {
                await SpawnNpcAsync(gameId, cancellationToken: cancellationToken);
            }

            // Process NPC behaviors
            await ProcessNpcBehaviorsAsync(gameId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NPC spawning for game {GameId}", gameId);
        }
    }

    /// <summary>
    /// Spawn a single NPC ship
    /// </summary>
    public async Task<int> SpawnNpcAsync(
        int gameId,
        int? specificSystemId = null,
        int? difficultyLevel = null,
        CancellationToken cancellationToken = default)
    {
        // Get a random system if not specified
        int systemId;
        if (specificSystemId.HasValue)
        {
            systemId = specificSystemId.Value;
        }
        else
        {
            var systems = await _context.StarSystems
                .Include(s => s.Galaxy)
                .Where(s => s.Galaxy.GameId == gameId)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            if (systems.Count == 0)
            {
                throw new InvalidOperationException("No systems available for NPC spawn");
            }

            systemId = systems[_random.Next(systems.Count)];
        }

        // Determine difficulty (1-10)
        int difficulty = difficultyLevel ?? _random.Next(1, 6); // Default 1-5

        // Select ship type based on difficulty
        var shipType = SelectNpcShipType(difficulty);

        // Get ship stats
        var shipStats = GetShipStats(shipType, difficulty);

        // Create spaceship
        var spaceship = new Spaceship
        {
            PlayerId = "NPC", // Special NPC player ID
            GameId = gameId,
            Name = GenerateNpcShipName(shipType),
            Type = shipType,
            State = ShipState.Active,
            CurrentSystemId = systemId,
            PositionX = _random.Next(-400, 401),
            PositionY = _random.Next(-400, 401),
            Speed = shipStats.Speed,
            Health = shipStats.MaxHealth,
            MaxHealth = shipStats.MaxHealth,
            Attack = shipStats.Attack,
            Defense = shipStats.Defense,
            CargoCapacity = 0,
            ConstructionProgress = 100,
            ConstructionCompletedTime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.Spaceships.Add(spaceship);
        await _context.SaveChangesAsync(cancellationToken);

        // Create NPC metadata
        var npcShip = new NpcShip
        {
            SpaceshipId = spaceship.Id,
            GameId = gameId,
            BehaviorType = SelectBehaviorType(difficulty),
            DifficultyLevel = difficulty,
            SpawnTime = DateTime.UtcNow,
            SpawnSystemId = systemId,
            LastBehaviorUpdate = DateTime.UtcNow,
            LootIronMin = difficulty * 5,
            LootIronMax = difficulty * 15,
            LootCopperMin = difficulty * 3,
            LootCopperMax = difficulty * 10,
            LootFuelMin = difficulty * 2,
            LootFuelMax = difficulty * 8
        };

        _context.NpcShips.Add(npcShip);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Spawned NPC ship {ShipId} ({Type}) in system {SystemId} for game {GameId}",
            spaceship.Id, shipType, systemId, gameId);

        return spaceship.Id;
    }

    /// <summary>
    /// Process behaviors for all NPC ships
    /// </summary>
    private async Task ProcessNpcBehaviorsAsync(int gameId, CancellationToken cancellationToken)
    {
        var npcShips = await _context.NpcShips
            .Include(n => n.Spaceship)
            .ThenInclude(s => s.CurrentSystem)
            .Where(n => n.GameId == gameId && n.Spaceship.Health > 0)
            .ToListAsync(cancellationToken);

        foreach (var npc in npcShips)
        {
            // Only update behavior every 10 seconds
            if ((DateTime.UtcNow - npc.LastBehaviorUpdate).TotalSeconds < 10)
                continue;

            await ProcessNpcBehaviorAsync(npc, cancellationToken);
            npc.LastBehaviorUpdate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Process behavior for a single NPC
    /// </summary>
    private async Task ProcessNpcBehaviorAsync(NpcShip npc, CancellationToken cancellationToken)
    {
        switch (npc.BehaviorType)
        {
            case NpcBehaviorType.Patrol:
                await ProcessPatrolBehaviorAsync(npc, cancellationToken);
                break;

            case NpcBehaviorType.Ambush:
                await ProcessAmbushBehaviorAsync(npc, cancellationToken);
                break;

            case NpcBehaviorType.Aggressive:
                await ProcessAggressiveBehaviorAsync(npc, cancellationToken);
                break;

            case NpcBehaviorType.Passive:
                // Passive NPCs don't move unless attacked
                break;
        }
    }

    /// <summary>
    /// Patrol behavior - move randomly within system
    /// </summary>
    private async Task ProcessPatrolBehaviorAsync(NpcShip npc, CancellationToken cancellationToken)
    {
        // If not currently moving, set new patrol target
        if (!npc.PatrolTargetX.HasValue || !npc.PatrolTargetY.HasValue ||
            IsNearTarget(npc.Spaceship, npc.PatrolTargetX.Value, npc.PatrolTargetY.Value))
        {
            // Set new random patrol point
            npc.PatrolTargetX = _random.Next(-500, 501);
            npc.PatrolTargetY = _random.Next(-500, 501);
        }

        // Move towards patrol target
        MoveTowardsTarget(npc.Spaceship, npc.PatrolTargetX.Value, npc.PatrolTargetY.Value);
    }

    /// <summary>
    /// Ambush behavior - wait near high-traffic areas
    /// </summary>
    private async Task ProcessAmbushBehaviorAsync(NpcShip npc, CancellationToken cancellationToken)
    {
        // Ambush NPCs stay near wormholes
        if (!npc.PatrolTargetX.HasValue)
        {
            // Pick a random wormhole position in the system
            var wormholes = await _context.Wormholes
                .Where(w => w.SystemAId == npc.Spaceship.CurrentSystemId || 
                           w.SystemBId == npc.Spaceship.CurrentSystemId)
                .ToListAsync(cancellationToken);

            if (wormholes.Any())
            {
                // Position near wormhole
                var randomOffset = _random.Next(-100, 101);
                npc.PatrolTargetX = randomOffset;
                npc.PatrolTargetY = randomOffset;
            }
        }

        // Stay at ambush position
        if (npc.PatrolTargetX.HasValue && npc.PatrolTargetY.HasValue)
        {
            MoveTowardsTarget(npc.Spaceship, npc.PatrolTargetX.Value, npc.PatrolTargetY.Value);
        }
    }

    /// <summary>
    /// Aggressive behavior - actively seek player ships
    /// </summary>
    private async Task ProcessAggressiveBehaviorAsync(NpcShip npc, CancellationToken cancellationToken)
    {
        // Find nearest player ship in system
        var playerShips = await _context.Spaceships
            .Where(s => s.GameId == npc.GameId &&
                       s.CurrentSystemId == npc.Spaceship.CurrentSystemId &&
                       s.PlayerId != "NPC" &&
                       s.Health > 0)
            .ToListAsync(cancellationToken);

        if (playerShips.Any())
        {
            // Find closest ship
            Spaceship? closestShip = null;
            double minDistance = double.MaxValue;

            foreach (var ship in playerShips)
            {
                var distance = CalculateDistance(
                    npc.Spaceship.PositionX, npc.Spaceship.PositionY,
                    ship.PositionX, ship.PositionY);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestShip = ship;
                }
            }

            if (closestShip != null)
            {
                npc.TargetShipId = closestShip.Id;
                MoveTowardsTarget(npc.Spaceship, closestShip.PositionX, closestShip.PositionY);
            }
        }
        else
        {
            // No targets, patrol randomly
            await ProcessPatrolBehaviorAsync(npc, cancellationToken);
        }
    }

    /// <summary>
    /// Move ship towards target position
    /// </summary>
    private void MoveTowardsTarget(Spaceship ship, double targetX, double targetY)
    {
        var dx = targetX - ship.PositionX;
        var dy = targetY - ship.PositionY;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance > 0)
        {
            // Move at ship speed (per tick = 5 seconds)
            var moveDistance = ship.Speed * 5; // Speed units per 5 seconds

            if (distance <= moveDistance)
            {
                // Reached target
                ship.PositionX = targetX;
                ship.PositionY = targetY;
            }
            else
            {
                // Move towards target
                ship.PositionX += (dx / distance) * moveDistance;
                ship.PositionY += (dy / distance) * moveDistance;
            }

            ship.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Check if ship is near target position
    /// </summary>
    private bool IsNearTarget(Spaceship ship, double targetX, double targetY)
    {
        var distance = CalculateDistance(ship.PositionX, ship.PositionY, targetX, targetY);
        return distance < 10.0; // Within 10 units
    }

    /// <summary>
    /// Calculate distance between two points
    /// </summary>
    private double CalculateDistance(double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Select NPC ship type based on difficulty
    /// </summary>
    private ShipType SelectNpcShipType(int difficulty)
    {
        return difficulty switch
        {
            <= 2 => ShipType.Scout,
            <= 4 => ShipType.Destroyer,
            <= 6 => ShipType.Cruiser,
            <= 8 => ShipType.Carrier,
            _ => ShipType.Capital
        };
    }

    /// <summary>
    /// Select behavior type based on difficulty
    /// </summary>
    private NpcBehaviorType SelectBehaviorType(int difficulty)
    {
        var roll = _random.NextDouble();

        if (difficulty <= 2)
        {
            return roll < 0.7 ? NpcBehaviorType.Patrol : NpcBehaviorType.Passive;
        }
        else if (difficulty <= 5)
        {
            return roll < 0.5 ? NpcBehaviorType.Patrol : 
                   roll < 0.8 ? NpcBehaviorType.Ambush : NpcBehaviorType.Aggressive;
        }
        else
        {
            return roll < 0.3 ? NpcBehaviorType.Ambush : NpcBehaviorType.Aggressive;
        }
    }

    /// <summary>
    /// Get ship stats based on type and difficulty
    /// </summary>
    private (int MaxHealth, int Attack, int Defense, double Speed) GetShipStats(ShipType type, int difficulty)
    {
        var difficultyMultiplier = 1.0 + (difficulty * 0.2); // 20% increase per difficulty level

        return type switch
        {
            ShipType.Scout => (
                MaxHealth: (int)(50 * difficultyMultiplier),
                Attack: (int)(10 * difficultyMultiplier),
                Defense: (int)(5 * difficultyMultiplier),
                Speed: 15.0
            ),
            ShipType.Destroyer => (
                MaxHealth: (int)(150 * difficultyMultiplier),
                Attack: (int)(40 * difficultyMultiplier),
                Defense: (int)(20 * difficultyMultiplier),
                Speed: 10.0
            ),
            ShipType.Cruiser => (
                MaxHealth: (int)(300 * difficultyMultiplier),
                Attack: (int)(60 * difficultyMultiplier),
                Defense: (int)(40 * difficultyMultiplier),
                Speed: 8.0
            ),
            ShipType.Carrier => (
                MaxHealth: (int)(400 * difficultyMultiplier),
                Attack: (int)(50 * difficultyMultiplier),
                Defense: (int)(60 * difficultyMultiplier),
                Speed: 6.0
            ),
            ShipType.Capital => (
                MaxHealth: (int)(600 * difficultyMultiplier),
                Attack: (int)(100 * difficultyMultiplier),
                Defense: (int)(80 * difficultyMultiplier),
                Speed: 5.0
            ),
            _ => (
                MaxHealth: (int)(100 * difficultyMultiplier),
                Attack: (int)(20 * difficultyMultiplier),
                Defense: (int)(10 * difficultyMultiplier),
                Speed: 10.0
            )
        };
    }

    /// <summary>
    /// Generate a random name for NPC ship
    /// </summary>
    private string GenerateNpcShipName(ShipType type)
    {
        var prefixes = new[] { "Crimson", "Shadow", "Void", "Rogue", "Dark", "Steel", "Iron", "Blood" };
        var suffixes = new[] { "Raider", "Marauder", "Reaver", "Corsair", "Brigand", "Bandit", "Pirate" };
        
        var prefix = prefixes[_random.Next(prefixes.Length)];
        var suffix = suffixes[_random.Next(suffixes.Length)];
        
        return $"{prefix} {suffix}";
    }
}
