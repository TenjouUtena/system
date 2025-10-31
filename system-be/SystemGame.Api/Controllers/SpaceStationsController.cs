using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Models;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/space-stations")]
[Authorize]
public class SpaceStationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SpaceStationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<List<SpaceStationDto>>> GetPlayerSpaceStations(int gameId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == gameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var spaceStations = await _context.SpaceStations
            .Include(ss => ss.Player)
            .Include(ss => ss.StarSystem)
            .Where(ss => ss.PlayerId == userId && ss.StarSystem.Galaxy.GameId == gameId)
            .ToListAsync();

        var stationDtos = spaceStations.Select(ss => new SpaceStationDto
        {
            Id = ss.Id,
            Name = ss.Name,
            SystemId = ss.SystemId,
            SystemName = ss.StarSystem?.Name ?? "",
            PlayerId = ss.PlayerId,
            PlayerName = ss.Player?.DisplayName ?? "",
            IronAmount = ss.IronAmount,
            CopperAmount = ss.CopperAmount,
            FuelAmount = ss.FuelAmount,
            SoilAmount = ss.SoilAmount,
            CreatedAt = ss.CreatedAt
        }).ToList();

        return Ok(stationDtos);
    }

    [HttpGet("system/{systemId}")]
    public async Task<ActionResult<List<SpaceStationDto>>> GetSpaceStationsForSystem(int systemId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var system = await _context.StarSystems
            .Include(s => s.Galaxy)
            .FirstOrDefaultAsync(s => s.Id == systemId);

        if (system == null)
        {
            return NotFound();
        }

        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == system.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var spaceStations = await _context.SpaceStations
            .Include(ss => ss.Player)
            .Include(ss => ss.StarSystem)
            .Where(ss => ss.SystemId == systemId)
            .ToListAsync();

        var stationDtos = spaceStations.Select(ss => new SpaceStationDto
        {
            Id = ss.Id,
            Name = ss.Name,
            SystemId = ss.SystemId,
            SystemName = ss.StarSystem?.Name ?? "",
            PlayerId = ss.PlayerId,
            PlayerName = ss.Player?.DisplayName ?? "",
            IronAmount = ss.IronAmount,
            CopperAmount = ss.CopperAmount,
            FuelAmount = ss.FuelAmount,
            SoilAmount = ss.SoilAmount,
            CreatedAt = ss.CreatedAt
        }).ToList();

        return Ok(stationDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpaceStationDto>> GetSpaceStation(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var spaceStation = await _context.SpaceStations
            .Include(ss => ss.Player)
            .Include(ss => ss.StarSystem)
                .ThenInclude(s => s.Galaxy)
            .FirstOrDefaultAsync(ss => ss.Id == id);

        if (spaceStation == null)
        {
            return NotFound();
        }

        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == spaceStation.StarSystem.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        return Ok(new SpaceStationDto
        {
            Id = spaceStation.Id,
            Name = spaceStation.Name,
            SystemId = spaceStation.SystemId,
            SystemName = spaceStation.StarSystem?.Name ?? "",
            PlayerId = spaceStation.PlayerId,
            PlayerName = spaceStation.Player?.DisplayName ?? "",
            IronAmount = spaceStation.IronAmount,
            CopperAmount = spaceStation.CopperAmount,
            FuelAmount = spaceStation.FuelAmount,
            SoilAmount = spaceStation.SoilAmount,
            CreatedAt = spaceStation.CreatedAt
        });
    }
}

