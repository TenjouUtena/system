using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/buildings")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BuildingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("place")]
    public async Task<ActionResult<BuildingDto>> PlaceBuilding([FromBody] PlaceBuildingRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Get the grid square
        var gridSquare = await _context.GridSquares
            .Include(g => g.PlanetGrid)
                .ThenInclude(pg => pg!.Planet)
                    .ThenInclude(p => p.StarSystem)
                        .ThenInclude(s => s.Galaxy)
            .FirstOrDefaultAsync(g => g.Id == request.GridSquareId);

        if (gridSquare == null)
        {
            return NotFound();
        }

        // Verify user is in the game
        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == gridSquare.PlanetGrid!.Planet.StarSystem.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        // Check if there's already a building on this square
        var existingBuilding = await _context.Buildings
            .FirstOrDefaultAsync(b => b.GridSquareId == request.GridSquareId);

        if (existingBuilding != null)
        {
            return BadRequest(new { message = "There is already a building on this square" });
        }

        // Get an available builder
        var builder = await _context.Builders
            .FirstOrDefaultAsync(b => b.PlanetId == gridSquare.PlanetGrid!.Planet.Id && 
                                     b.PlayerId == userId && 
                                     b.IsAvailable);

        if (builder == null)
        {
            return BadRequest(new { message = "No available builder on this planet" });
        }

        // Create the building
        var building = new Building
        {
            GridSquareId = request.GridSquareId,
            Type = (BuildingType)request.BuildingType,
            PlayerId = userId,
            ConstructionProgress = 0,
            ConstructionStartTime = DateTime.UtcNow,
            IsComplete = false
        };

        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();

        // Assign builder to building
        builder.AssignedBuildingId = building.Id;
        builder.IsAvailable = false;

        await _context.SaveChangesAsync();

        var buildingDto = new BuildingDto
        {
            Id = building.Id,
            GridSquareId = building.GridSquareId,
            Type = building.Type,
            PlayerId = building.PlayerId,
            PlayerName = building.Player?.DisplayName ?? "",
            ConstructionProgress = building.ConstructionProgress,
            IsComplete = building.IsComplete,
            ConstructionStartTime = building.ConstructionStartTime,
            X = gridSquare.X,
            Y = gridSquare.Y
        };

        return Ok(buildingDto);
    }

    [HttpGet("planet/{planetId}")]
    public async Task<ActionResult<List<BuildingDto>>> GetBuildingsForPlanet(int planetId)
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

        var buildings = await _context.Buildings
            .Include(b => b.Player)
            .Include(b => b.GridSquare)
            .Where(b => b.GridSquare.PlanetGrid!.PlanetId == planetId)
            .ToListAsync();

        var buildingDtos = buildings.Select(b => new BuildingDto
        {
            Id = b.Id,
            GridSquareId = b.GridSquareId,
            Type = b.Type,
            PlayerId = b.PlayerId,
            PlayerName = b.Player?.DisplayName ?? "",
            ConstructionProgress = b.ConstructionProgress,
            IsComplete = b.IsComplete,
            ConstructionStartTime = b.ConstructionStartTime,
            X = b.GridSquare.X,
            Y = b.GridSquare.Y
        }).ToList();

        return Ok(buildingDtos);
    }
}

