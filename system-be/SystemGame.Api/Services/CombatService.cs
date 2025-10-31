using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services;

/// <summary>
/// Service for managing combat between spaceships
/// </summary>
public class CombatService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CombatService> _logger;
    private static readonly Random _random = new Random();
    
    // Combat configuration
    private const double COMBAT_DETECTION_RANGE = 50.0;  // Units
    private const int MAX_COMBAT_ROUNDS = 20;
    private const double FLEE_CHANCE_BASE = 0.15;  // 15% base flee chance per round

    public CombatService(
        ApplicationDbContext context,
        ILogger<CombatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Process combat for all active battles in a game
    /// </summary>
    public async Task ProcessCombatAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Detect new combats
            await DetectNewCombatsAsync(gameId, cancellationToken);

            // Process ongoing battles
            await ProcessOngoingBattlesAsync(gameId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing combat for game {GameId}", gameId);
        }
    }

    /// <summary>
    /// Detect ships in proximity and start battles
    /// </summary>
    private async Task DetectNewCombatsAsync(int gameId, CancellationToken cancellationToken)
    {
        // Get all active ships in this game
        var activeShips = await _context.Spaceships
            .Include(s => s.Player)
            .Where(s => s.GameId == gameId && 
                       s.State == ShipState.Active && 
                       s.Health > 0)
            .ToListAsync(cancellationToken);

        // Also get NPC ships
        var npcShips = await _context.NpcShips
            .Include(n => n.Spaceship)
            .ThenInclude(s => s.Player)
            .Where(n => n.GameId == gameId && 
                       n.Spaceship.Health > 0 &&
                       n.Spaceship.State == ShipState.Active)
            .Select(n => n.Spaceship)
            .ToListAsync(cancellationToken);

        var allShips = activeShips.Concat(npcShips).ToList();

        // Check for proximity-based combat
        for (int i = 0; i < allShips.Count; i++)
        {
            for (int j = i + 1; j < allShips.Count; j++)
            {
                var ship1 = allShips[i];
                var ship2 = allShips[j];

                // Ships must be in same system
                if (ship1.CurrentSystemId != ship2.CurrentSystemId)
                    continue;

                // Ships from same player don't fight
                if (ship1.PlayerId == ship2.PlayerId)
                    continue;

                // Check if already in a battle
                var existingBattle = await _context.BattleParticipants
                    .Include(bp => bp.Battle)
                    .Where(bp => (bp.SpaceshipId == ship1.Id || bp.SpaceshipId == ship2.Id) &&
                                bp.Battle.State == BattleState.InProgress)
                    .Select(bp => bp.Battle)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingBattle != null)
                    continue;

                // Calculate distance
                var distance = CalculateDistance(
                    ship1.PositionX, ship1.PositionY,
                    ship2.PositionX, ship2.PositionY);

                // Start combat if in range
                if (distance <= COMBAT_DETECTION_RANGE)
                {
                    await InitiateBattleAsync(ship1, ship2, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Initiate a new battle between two ships
    /// </summary>
    private async Task InitiateBattleAsync(
        Spaceship ship1, 
        Spaceship ship2, 
        CancellationToken cancellationToken)
    {
        // Create battle
        var battle = new Battle
        {
            GameId = ship1.GameId,
            SystemId = ship1.CurrentSystemId,
            PositionX = (ship1.PositionX + ship2.PositionX) / 2,
            PositionY = (ship1.PositionY + ship2.PositionY) / 2,
            State = BattleState.InProgress,
            StartTime = DateTime.UtcNow
        };

        _context.Battles.Add(battle);
        await _context.SaveChangesAsync(cancellationToken);

        // Add participants
        var participant1 = CreateBattleParticipant(battle.Id, ship1);
        var participant2 = CreateBattleParticipant(battle.Id, ship2);

        _context.BattleParticipants.AddRange(participant1, participant2);

        // Log battle start
        var startEvent = new BattleEvent
        {
            BattleId = battle.Id,
            Round = 0,
            Type = BattleEventType.BattleStarted,
            Description = $"Battle initiated between {ship1.Name} and {ship2.Name}",
            Timestamp = DateTime.UtcNow
        };
        _context.BattleEvents.Add(startEvent);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Battle {BattleId} started between ships {Ship1} and {Ship2}",
            battle.Id, ship1.Id, ship2.Id);
    }

    /// <summary>
    /// Process all ongoing battles
    /// </summary>
    private async Task ProcessOngoingBattlesAsync(int gameId, CancellationToken cancellationToken)
    {
        var ongoingBattles = await _context.Battles
            .Include(b => b.Participants)
                .ThenInclude(p => p.Spaceship)
            .Include(b => b.Events)
            .Where(b => b.GameId == gameId && b.State == BattleState.InProgress)
            .ToListAsync(cancellationToken);

        foreach (var battle in ongoingBattles)
        {
            await ProcessBattleRoundAsync(battle, cancellationToken);
        }
    }

    /// <summary>
    /// Process a single round of combat
    /// </summary>
    private async Task ProcessBattleRoundAsync(Battle battle, CancellationToken cancellationToken)
    {
        battle.RoundsElapsed++;

        // Get alive participants
        var aliveParticipants = battle.Participants
            .Where(p => p.Spaceship.Health > 0 && !p.Fled)
            .ToList();

        // Check victory conditions
        if (aliveParticipants.Count <= 1 || battle.RoundsElapsed >= MAX_COMBAT_ROUNDS)
        {
            await EndBattleAsync(battle, cancellationToken);
            return;
        }

        // Check for fleeing
        foreach (var participant in aliveParticipants)
        {
            // Chance to flee increases with damage taken
            var healthPercent = (double)participant.Spaceship.Health / participant.Spaceship.MaxHealth;
            var fleeChance = FLEE_CHANCE_BASE * (1.0 - healthPercent);

            if (_random.NextDouble() < fleeChance)
            {
                participant.Fled = true;
                participant.Spaceship.State = ShipState.Fleeing;

                var fleeEvent = new BattleEvent
                {
                    BattleId = battle.Id,
                    Round = battle.RoundsElapsed,
                    Type = BattleEventType.ShipFled,
                    AttackerShipId = participant.SpaceshipId,
                    Description = $"{participant.Spaceship.Name} fled from battle!",
                    Timestamp = DateTime.UtcNow
                };
                _context.BattleEvents.Add(fleeEvent);

                _logger.LogInformation("Ship {ShipId} fled from battle {BattleId}", 
                    participant.SpaceshipId, battle.Id);
            }
        }

        // Refresh alive participants after fleeing
        aliveParticipants = battle.Participants
            .Where(p => p.Spaceship.Health > 0 && !p.Fled)
            .ToList();

        if (aliveParticipants.Count <= 1)
        {
            await EndBattleAsync(battle, cancellationToken);
            return;
        }

        // Each ship attacks a random enemy
        foreach (var attacker in aliveParticipants)
        {
            var enemies = aliveParticipants
                .Where(p => p.PlayerId != attacker.PlayerId)
                .ToList();

            if (enemies.Count == 0)
                continue;

            var target = enemies[_random.Next(enemies.Count)];

            // Calculate damage
            var damage = CalculateDamage(
                attacker.Spaceship.Attack,
                target.Spaceship.Defense);

            // Apply damage
            target.Spaceship.Health = Math.Max(0, target.Spaceship.Health - damage);
            target.DamageTaken += damage;
            attacker.DamageDealt += damage;

            // Log attack event
            var attackEvent = new BattleEvent
            {
                BattleId = battle.Id,
                Round = battle.RoundsElapsed,
                Type = BattleEventType.Attack,
                AttackerShipId = attacker.SpaceshipId,
                DefenderShipId = target.SpaceshipId,
                DamageDealt = damage,
                Description = $"{attacker.Spaceship.Name} attacked {target.Spaceship.Name} for {damage} damage",
                Timestamp = DateTime.UtcNow
            };
            _context.BattleEvents.Add(attackEvent);

            // Check if target destroyed
            if (target.Spaceship.Health <= 0)
            {
                target.Survived = false;
                target.Spaceship.State = ShipState.Destroyed;

                var destroyEvent = new BattleEvent
                {
                    BattleId = battle.Id,
                    Round = battle.RoundsElapsed,
                    Type = BattleEventType.ShipDestroyed,
                    AttackerShipId = attacker.SpaceshipId,
                    DefenderShipId = target.SpaceshipId,
                    Description = $"{target.Spaceship.Name} was destroyed by {attacker.Spaceship.Name}!",
                    Timestamp = DateTime.UtcNow
                };
                _context.BattleEvents.Add(destroyEvent);

                _logger.LogInformation("Ship {ShipId} destroyed in battle {BattleId}", 
                    target.SpaceshipId, battle.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// End a battle and distribute rewards
    /// </summary>
    private async Task EndBattleAsync(Battle battle, CancellationToken cancellationToken)
    {
        battle.State = BattleState.Completed;
        battle.EndTime = DateTime.UtcNow;

        // Determine winner
        var survivors = battle.Participants
            .Where(p => p.Spaceship.Health > 0 && !p.Fled)
            .ToList();

        if (survivors.Count == 1)
        {
            battle.WinnerPlayerId = survivors[0].PlayerId;
            battle.EndReason = BattleEndReason.AllEnemiesDestroyed;
        }
        else if (battle.RoundsElapsed >= MAX_COMBAT_ROUNDS)
        {
            battle.EndReason = BattleEndReason.Timeout;
        }
        else
        {
            battle.EndReason = BattleEndReason.OneSideFled;
        }

        // Distribute rewards to survivors
        foreach (var survivor in survivors)
        {
            // Experience based on damage dealt
            survivor.ExperienceGained = survivor.DamageDealt;

            // Loot from destroyed enemies
            var destroyedEnemies = battle.Participants
                .Where(p => !p.Survived && p.PlayerId != survivor.PlayerId)
                .ToList();

            foreach (var enemy in destroyedEnemies)
            {
                // Check if enemy is NPC with loot
                var npcData = await _context.NpcShips
                    .FirstOrDefaultAsync(n => n.SpaceshipId == enemy.SpaceshipId, cancellationToken);

                if (npcData != null)
                {
                    // Award loot
                    survivor.LootIron = (survivor.LootIron ?? 0) + _random.Next(npcData.LootIronMin, npcData.LootIronMax + 1);
                    survivor.LootCopper = (survivor.LootCopper ?? 0) + _random.Next(npcData.LootCopperMin, npcData.LootCopperMax + 1);
                    survivor.LootFuel = (survivor.LootFuel ?? 0) + _random.Next(npcData.LootFuelMin, npcData.LootFuelMax + 1);
                }
            }
        }

        // Log battle end
        var endEvent = new BattleEvent
        {
            BattleId = battle.Id,
            Round = battle.RoundsElapsed,
            Type = BattleEventType.BattleEnded,
            Description = $"Battle ended. Winner: {battle.WinnerPlayerId ?? "Draw"}",
            Timestamp = DateTime.UtcNow
        };
        _context.BattleEvents.Add(endEvent);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Battle {BattleId} ended. Winner: {Winner}", 
            battle.Id, battle.WinnerPlayerId ?? "Draw");
    }

    /// <summary>
    /// Calculate damage dealt by attacker to defender
    /// </summary>
    private int CalculateDamage(int attack, int defense)
    {
        // Base damage with randomness
        var baseDamage = attack * (0.8 + _random.NextDouble() * 0.4); // 80-120% of attack

        // Defense reduces damage
        var damageReduction = defense * 0.5; // 50% efficiency
        var finalDamage = Math.Max(1, baseDamage - damageReduction);

        return (int)finalDamage;
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
    /// Create a battle participant from a spaceship
    /// </summary>
    private BattleParticipant CreateBattleParticipant(int battleId, Spaceship ship)
    {
        // Check if ship is NPC
        var isNpc = _context.NpcShips.Any(n => n.SpaceshipId == ship.Id);

        return new BattleParticipant
        {
            BattleId = battleId,
            SpaceshipId = ship.Id,
            PlayerId = ship.PlayerId,
            IsNpc = isNpc,
            InitialHealth = ship.Health,
            FinalHealth = ship.Health,
            Attack = ship.Attack,
            Defense = ship.Defense,
            Survived = true,
            Fled = false
        };
    }
}
