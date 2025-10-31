using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;
using SystemGame.Api.Services;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly GalaxyGeneratorService _galaxyGenerator;
    private readonly PlanetGridGeneratorService _planetGridGenerator;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        ApplicationDbContext context,
        GalaxyGeneratorService galaxyGenerator,
        PlanetGridGeneratorService planetGridGenerator,
        ILogger<GamesController> logger)
    {
        _context = context;
        _galaxyGenerator = galaxyGenerator;
        _planetGridGenerator = planetGridGenerator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<GameDto>>> GetGames()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var games = await _context.Games
            .Include(g => g.Players)
            .Include(g => g.Galaxy)
            .Where(g => g.IsActive)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var gameDtos = games.Select(g => new GameDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            PlayerCount = g.Players.Count,
            MaxPlayers = g.MaxPlayers,
            SystemCount = g.Galaxy?.SystemCount ?? 0,
            IsActive = g.IsActive,
            CreatedAt = g.CreatedAt,
            IsJoined = g.Players.Any(p => p.UserId == userId),
            IsCreator = g.CreatorId == userId
        }).ToList();

        return Ok(gameDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetGame(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var game = await _context.Games
            .Include(g => g.Players)
            .Include(g => g.Galaxy)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
        {
            return NotFound();
        }

        return Ok(new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Description = game.Description,
            PlayerCount = game.Players.Count,
            MaxPlayers = game.MaxPlayers,
            SystemCount = game.Galaxy?.SystemCount ?? 0,
            IsActive = game.IsActive,
            CreatedAt = game.CreatedAt,
            IsJoined = game.Players.Any(p => p.UserId == userId),
            IsCreator = game.CreatorId == userId
        });
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            // Create the game
            var game = new Game
            {
                Name = request.Name,
                Description = request.Description,
                CreatorId = userId,
                MaxPlayers = request.MaxPlayers,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Generate galaxy
            _logger.LogInformation("Generating galaxy for game {GameId} with {SystemCount} systems", 
                game.Id, request.SystemCount);
            
            await _galaxyGenerator.GenerateGalaxyAsync(game.Id, $"{game.Name} Galaxy", request.SystemCount, _planetGridGenerator);

            // Add creator as player
            var playerGame = new PlayerGame
            {
                UserId = userId,
                GameId = game.Id,
                JoinedAt = DateTime.UtcNow
            };

            _context.PlayerGames.Add(playerGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Game {GameId} created successfully", game.Id);

            // Reload to get the galaxy
            await _context.Entry(game).Reference(g => g.Galaxy).LoadAsync();

            return Ok(new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                PlayerCount = 1,
                MaxPlayers = game.MaxPlayers,
                SystemCount = game.Galaxy?.SystemCount ?? request.SystemCount,
                IsActive = game.IsActive,
                CreatedAt = game.CreatedAt,
                IsJoined = true,
                IsCreator = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game");
            return StatusCode(500, new { message = "Failed to create game" });
        }
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult> JoinGame(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var game = await _context.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

        if (game == null)
        {
            return NotFound();
        }

        if (game.Players.Any(p => p.UserId == userId))
        {
            return BadRequest(new { message = "Already in this game" });
        }

        if (game.Players.Count >= game.MaxPlayers)
        {
            return BadRequest(new { message = "Game is full" });
        }

        var playerGame = new PlayerGame
        {
            UserId = userId,
            GameId = game.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.PlayerGames.Add(playerGame);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully joined game" });
    }

    [HttpGet("{id}/galaxy")]
    public async Task<ActionResult<GalaxyMapDto>> GetGalaxyMap(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify user is in the game
        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == id && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var galaxy = await _context.Galaxies
            .Include(g => g.Systems)
            .ThenInclude(s => s.Planets)
            .FirstOrDefaultAsync(g => g.GameId == id);

        if (galaxy == null)
        {
            return NotFound();
        }

        var systems = await _context.StarSystems
            .Where(s => s.GalaxyId == galaxy.Id)
            .ToListAsync();

        var wormholes = await _context.Wormholes
            .Where(w => w.SystemA.GalaxyId == galaxy.Id || w.SystemB.GalaxyId == galaxy.Id)
            .ToListAsync();

        var galaxyMap = new GalaxyMapDto
        {
            Id = galaxy.Id,
            Name = galaxy.Name,
            Systems = systems.Select(s => new SystemMapDto
            {
                Id = s.Id,
                Name = s.Name,
                X = s.X,
                Y = s.Y,
                PlanetCount = s.Planets.Count
            }).ToList(),
            Wormholes = wormholes.Select(w => new WormholeMapDto
            {
                Id = w.Id,
                SystemAId = w.SystemAId,
                SystemBId = w.SystemBId,
                IsActive = w.IsActive
            }).ToList()
        };

        return Ok(galaxyMap);
    }

    [HttpGet("{id}/systems/{systemId}")]
    public async Task<ActionResult<SystemDetailDto>> GetSystemDetail(int id, int systemId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify user is in the game
        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == id && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var system = await _context.StarSystems
            .Include(s => s.Planets)
            .FirstOrDefaultAsync(s => s.Id == systemId && s.Galaxy.GameId == id);

        if (system == null)
        {
            return NotFound();
        }

        var systemDetail = new SystemDetailDto
        {
            Id = system.Id,
            Name = system.Name,
            X = system.X,
            Y = system.Y,
            Planets = system.Planets.Select(p => new PlanetDto
            {
                Id = p.Id,
                Name = p.Name,
                Size = p.Size,
                Type = p.Type.ToString()
            }).ToList()
        };

        return Ok(systemDetail);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGame(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var game = await _context.Games
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
        {
            return NotFound();
        }

        // Only the creator can delete the game
        if (game.CreatorId != userId)
        {
            return Forbid();
        }

        try
        {
            // Get the galaxy for this game
            var galaxy = await _context.Galaxies
                .Include(g => g.Systems)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (galaxy != null)
            {
                // Get all system IDs in this galaxy
                var systemIds = galaxy.Systems.Select(s => s.Id).ToList();

                // Delete all wormholes where both systems are in this galaxy
                var wormholesToDelete = await _context.Wormholes
                    .Where(w => systemIds.Contains(w.SystemAId) && systemIds.Contains(w.SystemBId))
                    .ToListAsync();

                _context.Wormholes.RemoveRange(wormholesToDelete);
                _logger.LogInformation("Deleted {Count} wormholes from galaxy {GalaxyId}", 
                    wormholesToDelete.Count, galaxy.Id);
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Game {GameId} deleted by user {UserId}", id, userId);
            
            return Ok(new { message = "Game deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game {GameId}", id);
            return StatusCode(500, new { message = "Failed to delete game" });
        }
    }
}

