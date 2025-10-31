using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Models;
using SystemGame.Api.Services;

namespace SystemGame.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CombatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CombatController> _logger;
    private readonly NpcSpawnService _npcSpawnService;

    public CombatController(
        ApplicationDbContext context,
        ILogger<CombatController> logger,
        NpcSpawnService npcSpawnService)
    {
        _context = context;
        _logger = logger;
        _npcSpawnService = npcSpawnService;
    }

    /// <summary>
    /// Get all battles for a game
    /// </summary>
    [HttpGet("battles/game/{gameId}")]
    public async Task<ActionResult<List<BattleSummaryDto>>> GetBattlesByGame(int gameId)
    {
        try
        {
            var battles = await _context.Battles
                .Include(b => b.System)
                .Include(b => b.WinnerPlayer)
                .Include(b => b.Participants)
                .Where(b => b.GameId == gameId)
                .OrderByDescending(b => b.StartTime)
                .Select(b => new BattleSummaryDto
                {
                    Id = b.Id,
                    GameId = b.GameId,
                    SystemId = b.SystemId,
                    SystemName = b.System.Name,
                    State = b.State.ToString(),
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    RoundsElapsed = b.RoundsElapsed,
                    WinnerPlayerName = b.WinnerPlayer != null ? b.WinnerPlayer.DisplayName : null,
                    ParticipantCount = b.Participants.Count
                })
                .ToListAsync();

            return Ok(battles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting battles for game {GameId}", gameId);
            return StatusCode(500, "Error retrieving battles");
        }
    }

    /// <summary>
    /// Get detailed battle information
    /// </summary>
    [HttpGet("battles/{id}")]
    public async Task<ActionResult<BattleDto>> GetBattle(int id)
    {
        try
        {
            var battle = await _context.Battles
                .Include(b => b.System)
                .Include(b => b.WinnerPlayer)
                .Include(b => b.Participants)
                    .ThenInclude(p => p.Spaceship)
                .Include(b => b.Participants)
                    .ThenInclude(p => p.Player)
                .Include(b => b.Events)
                    .ThenInclude(e => e.AttackerShip)
                .Include(b => b.Events)
                    .ThenInclude(e => e.DefenderShip)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (battle == null)
            {
                return NotFound();
            }

            var battleDto = new BattleDto
            {
                Id = battle.Id,
                GameId = battle.GameId,
                SystemId = battle.SystemId,
                SystemName = battle.System.Name,
                State = battle.State.ToString(),
                PositionX = battle.PositionX,
                PositionY = battle.PositionY,
                StartTime = battle.StartTime,
                EndTime = battle.EndTime,
                RoundsElapsed = battle.RoundsElapsed,
                WinnerPlayerId = battle.WinnerPlayerId,
                WinnerPlayerName = battle.WinnerPlayer?.DisplayName,
                EndReason = battle.EndReason?.ToString(),
                CreatedAt = battle.CreatedAt,
                Participants = battle.Participants.Select(p => new BattleParticipantDto
                {
                    Id = p.Id,
                    BattleId = p.BattleId,
                    SpaceshipId = p.SpaceshipId,
                    SpaceshipName = p.Spaceship.Name,
                    PlayerId = p.PlayerId,
                    PlayerName = p.Player.DisplayName,
                    IsNpc = p.IsNpc,
                    InitialHealth = p.InitialHealth,
                    FinalHealth = p.FinalHealth,
                    Attack = p.Attack,
                    Defense = p.Defense,
                    DamageDealt = p.DamageDealt,
                    DamageTaken = p.DamageTaken,
                    Survived = p.Survived,
                    Fled = p.Fled,
                    ExperienceGained = p.ExperienceGained,
                    LootIron = p.LootIron,
                    LootCopper = p.LootCopper,
                    LootFuel = p.LootFuel
                }).ToList(),
                Events = battle.Events.OrderBy(e => e.Round).ThenBy(e => e.Timestamp).Select(e => new BattleEventDto
                {
                    Id = e.Id,
                    BattleId = e.BattleId,
                    Round = e.Round,
                    Type = e.Type.ToString(),
                    AttackerShipId = e.AttackerShipId,
                    AttackerShipName = e.AttackerShip?.Name,
                    DefenderShipId = e.DefenderShipId,
                    DefenderShipName = e.DefenderShip?.Name,
                    DamageDealt = e.DamageDealt,
                    Description = e.Description,
                    Timestamp = e.Timestamp
                }).ToList()
            };

            return Ok(battleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting battle {BattleId}", id);
            return StatusCode(500, "Error retrieving battle details");
        }
    }

    /// <summary>
    /// Get active battles in a system
    /// </summary>
    [HttpGet("battles/system/{systemId}/active")]
    public async Task<ActionResult<List<BattleSummaryDto>>> GetActiveBattlesBySystem(int systemId)
    {
        try
        {
            var battles = await _context.Battles
                .Include(b => b.System)
                .Include(b => b.WinnerPlayer)
                .Include(b => b.Participants)
                .Where(b => b.SystemId == systemId && b.State == Data.Entities.BattleState.InProgress)
                .OrderByDescending(b => b.StartTime)
                .Select(b => new BattleSummaryDto
                {
                    Id = b.Id,
                    GameId = b.GameId,
                    SystemId = b.SystemId,
                    SystemName = b.System.Name,
                    State = b.State.ToString(),
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    RoundsElapsed = b.RoundsElapsed,
                    WinnerPlayerName = b.WinnerPlayer != null ? b.WinnerPlayer.DisplayName : null,
                    ParticipantCount = b.Participants.Count
                })
                .ToListAsync();

            return Ok(battles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active battles for system {SystemId}", systemId);
            return StatusCode(500, "Error retrieving active battles");
        }
    }

    /// <summary>
    /// Get all NPC ships in a game
    /// </summary>
    [HttpGet("npcs/game/{gameId}")]
    public async Task<ActionResult<List<NpcShipDto>>> GetNpcShipsByGame(int gameId)
    {
        try
        {
            var npcShips = await _context.NpcShips
                .Include(n => n.Spaceship)
                    .ThenInclude(s => s.CurrentSystem)
                .Include(n => n.SpawnSystem)
                .Include(n => n.TargetShip)
                .Where(n => n.GameId == gameId && n.Spaceship.Health > 0)
                .ToListAsync();

            var npcDtos = npcShips.Select(n => new NpcShipDto
            {
                Id = n.Id,
                SpaceshipId = n.SpaceshipId,
                GameId = n.GameId,
                BehaviorType = n.BehaviorType.ToString(),
                DifficultyLevel = n.DifficultyLevel,
                PatrolTargetX = n.PatrolTargetX,
                PatrolTargetY = n.PatrolTargetY,
                TargetShipId = n.TargetShipId,
                TargetShipName = n.TargetShip?.Name,
                SpawnTime = n.SpawnTime,
                SpawnSystemId = n.SpawnSystemId,
                SpawnSystemName = n.SpawnSystem?.Name,
                LootIronMin = n.LootIronMin,
                LootIronMax = n.LootIronMax,
                LootCopperMin = n.LootCopperMin,
                LootCopperMax = n.LootCopperMax,
                LootFuelMin = n.LootFuelMin,
                LootFuelMax = n.LootFuelMax,
                Spaceship = new SpaceshipDto
                {
                    Id = n.Spaceship.Id,
                    PlayerId = n.Spaceship.PlayerId,
                    GameId = n.Spaceship.GameId,
                    Name = n.Spaceship.Name,
                    Type = n.Spaceship.Type.ToString(),
                    State = n.Spaceship.State.ToString(),
                    CurrentSystemId = n.Spaceship.CurrentSystemId,
                    CurrentSystemName = n.Spaceship.CurrentSystem.Name,
                    PositionX = n.Spaceship.PositionX,
                    PositionY = n.Spaceship.PositionY,
                    Speed = n.Spaceship.Speed,
                    Health = n.Spaceship.Health,
                    MaxHealth = n.Spaceship.MaxHealth,
                    Attack = n.Spaceship.Attack,
                    Defense = n.Spaceship.Defense,
                    CargoCapacity = n.Spaceship.CargoCapacity,
                    CreatedAt = n.Spaceship.CreatedAt,
                    LastUpdatedAt = n.Spaceship.LastUpdatedAt
                }
            }).ToList();

            return Ok(npcDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting NPC ships for game {GameId}", gameId);
            return StatusCode(500, "Error retrieving NPC ships");
        }
    }

    /// <summary>
    /// Spawn a new NPC ship (admin/testing endpoint)
    /// </summary>
    [HttpPost("npcs/spawn")]
    public async Task<ActionResult<NpcShipDto>> SpawnNpc([FromBody] SpawnNpcRequest request, [FromQuery] int gameId)
    {
        try
        {
            // In production, you might want to restrict this to admin users
            var spaceshipId = await _npcSpawnService.SpawnNpcAsync(
                gameId,
                request.SystemId,
                request.DifficultyLevel);

            var npcShip = await _context.NpcShips
                .Include(n => n.Spaceship)
                    .ThenInclude(s => s.CurrentSystem)
                .Include(n => n.SpawnSystem)
                .FirstOrDefaultAsync(n => n.SpaceshipId == spaceshipId);

            if (npcShip == null)
            {
                return NotFound();
            }

            var npcDto = new NpcShipDto
            {
                Id = npcShip.Id,
                SpaceshipId = npcShip.SpaceshipId,
                GameId = npcShip.GameId,
                BehaviorType = npcShip.BehaviorType.ToString(),
                DifficultyLevel = npcShip.DifficultyLevel,
                SpawnTime = npcShip.SpawnTime,
                SpawnSystemId = npcShip.SpawnSystemId,
                SpawnSystemName = npcShip.SpawnSystem?.Name,
                LootIronMin = npcShip.LootIronMin,
                LootIronMax = npcShip.LootIronMax,
                LootCopperMin = npcShip.LootCopperMin,
                LootCopperMax = npcShip.LootCopperMax,
                LootFuelMin = npcShip.LootFuelMin,
                LootFuelMax = npcShip.LootFuelMax,
                Spaceship = new SpaceshipDto
                {
                    Id = npcShip.Spaceship.Id,
                    PlayerId = npcShip.Spaceship.PlayerId,
                    GameId = npcShip.Spaceship.GameId,
                    Name = npcShip.Spaceship.Name,
                    Type = npcShip.Spaceship.Type.ToString(),
                    State = npcShip.Spaceship.State.ToString(),
                    CurrentSystemId = npcShip.Spaceship.CurrentSystemId,
                    CurrentSystemName = npcShip.Spaceship.CurrentSystem.Name,
                    PositionX = npcShip.Spaceship.PositionX,
                    PositionY = npcShip.Spaceship.PositionY,
                    Speed = npcShip.Spaceship.Speed,
                    Health = npcShip.Spaceship.Health,
                    MaxHealth = npcShip.Spaceship.MaxHealth,
                    Attack = npcShip.Spaceship.Attack,
                    Defense = npcShip.Spaceship.Defense,
                    CargoCapacity = npcShip.Spaceship.CargoCapacity,
                    CreatedAt = npcShip.Spaceship.CreatedAt,
                    LastUpdatedAt = npcShip.Spaceship.LastUpdatedAt
                }
            };

            return CreatedAtAction(nameof(GetNpcShipsByGame), new { gameId = npcShip.GameId }, npcDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error spawning NPC for game {GameId}", gameId);
            return StatusCode(500, "Error spawning NPC");
        }
    }
}
