using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Models;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/planets")]
[Authorize]
public class PlanetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PlanetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}/grid")]
    public async Task<ActionResult<PlanetGridDto>> GetPlanetGrid(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Get planet with grid and verify player has access to the game
        var planet = await _context.Planets
            .Include(p => p.Grid)
                .ThenInclude(g => g!.Squares)
            .Include(p => p.StarSystem)
                .ThenInclude(s => s.Galaxy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (planet == null)
        {
            return NotFound();
        }

        // Check if user is in the game
        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == planet.StarSystem.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        if (planet.Grid == null)
        {
            return NotFound(new { message = "Planet grid not generated yet" });
        }

        var gridDto = new PlanetGridDto
        {
            Id = planet.Grid.Id,
            PlanetId = planet.Id,
            Width = planet.Grid.Width,
            Height = planet.Grid.Height,
            Squares = planet.Grid.Squares.OrderBy(s => s.Y).ThenBy(s => s.X)
                .Select(s => new GridSquareDto
                {
                    Id = s.Id,
                    X = s.X,
                    Y = s.Y,
                    IronAmount = s.IronAmount,
                    CopperAmount = s.CopperAmount,
                    FuelAmount = s.FuelAmount,
                    SoilAmount = s.SoilAmount
                }).ToList()
        };

        return Ok(gridDto);
    }

    [HttpGet("{id}/grid/summary")]
    public async Task<ActionResult> GetPlanetGridSummary(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var planet = await _context.Planets
            .Include(p => p.Grid)
                .ThenInclude(g => g!.Squares)
            .Include(p => p.StarSystem)
                .ThenInclude(s => s.Galaxy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (planet == null || planet.Grid == null)
        {
            return NotFound();
        }

        var isInGame = await _context.PlayerGames
            .AnyAsync(pg => pg.GameId == planet.StarSystem.Galaxy.GameId && pg.UserId == userId);

        if (!isInGame)
        {
            return Forbid();
        }

        var totalIron = planet.Grid.Squares.Where(s => s.IronAmount.HasValue).Sum(s => s.IronAmount!.Value);
        var totalCopper = planet.Grid.Squares.Where(s => s.CopperAmount.HasValue).Sum(s => s.CopperAmount!.Value);
        var totalFuel = planet.Grid.Squares.Where(s => s.FuelAmount.HasValue).Sum(s => s.FuelAmount!.Value);
        var totalSoil = planet.Grid.Squares.Where(s => s.SoilAmount.HasValue).Sum(s => s.SoilAmount!.Value);

        return Ok(new
        {
            PlanetId = planet.Id,
            PlanetName = planet.Name,
            GridSize = planet.Grid.Width,
            TotalIron = totalIron,
            TotalCopper = totalCopper,
            TotalFuel = totalFuel,
            TotalSoil = totalSoil,
            IronSquares = planet.Grid.Squares.Count(s => s.IronAmount.HasValue),
            CopperSquares = planet.Grid.Squares.Count(s => s.CopperAmount.HasValue),
            FuelSquares = planet.Grid.Squares.Count(s => s.FuelAmount.HasValue),
            SoilSquares = planet.Grid.Squares.Count(s => s.SoilAmount.HasValue)
        });
    }
}

