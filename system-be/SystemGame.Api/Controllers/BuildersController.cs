using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/builders")]
[Authorize]
public class BuildersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BuildersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("planet/{planetId}")]
    public async Task<ActionResult<List<BuilderDto>>> GetBuildersForPlanet(int planetId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var planet = await _context.Planets
            .Include(p => p.StarSystem)
                .ThenInclude(s => s.Galaxy)
            .FirstOrDefaultAsync(p => p.Id == planetId);

        if (planet == null)
        {
            return NotFound();
        }

        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == planet.StarSystem.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var builders = await _context.Builders
            .Where(b => b.PlanetId == planetId && b.PlayerId == userId)
            .ToListAsync();

        var builderDtos = builders.Select(b => new BuilderDto
        {
            Id = b.Id,
            Name = b.Name,
            IsAvailable = b.IsAvailable,
            AssignedBuildingId = b.AssignedBuildingId,
            CreatedAt = b.CreatedAt
        }).ToList();

        return Ok(builderDtos);
    }

    [HttpPost("create")]
    public async Task<ActionResult<BuilderDto>> CreateBuilder([FromBody] CreateBuilderRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var planet = await _context.Planets
            .Include(p => p.StarSystem)
                .ThenInclude(s => s.Galaxy)
            .FirstOrDefaultAsync(p => p.Id == request.PlanetId);

        if (planet == null)
        {
            return NotFound();
        }

        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == planet.StarSystem.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var builder = new Builder
        {
            PlanetId = request.PlanetId,
            PlayerId = userId,
            Name = request.Name,
            IsAvailable = true
        };

        _context.Builders.Add(builder);
        await _context.SaveChangesAsync();

        return Ok(new BuilderDto
        {
            Id = builder.Id,
            Name = builder.Name,
            IsAvailable = builder.IsAvailable,
            AssignedBuildingId = builder.AssignedBuildingId,
            CreatedAt = builder.CreatedAt
        });
    }
}

