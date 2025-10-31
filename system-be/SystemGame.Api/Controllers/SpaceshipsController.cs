using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemGame.Api.Models;
using SystemGame.Api.Services;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/spaceships")]
[Authorize]
public class SpaceshipsController : ControllerBase
{
    private readonly SpaceshipService _spaceshipService;
    private readonly ILogger<SpaceshipsController> _logger;

    public SpaceshipsController(
        SpaceshipService spaceshipService,
        ILogger<SpaceshipsController> logger)
    {
        _spaceshipService = spaceshipService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) 
        ?? throw new UnauthorizedAccessException("User ID not found");

    #region Shipyard Endpoints

    /// <summary>
    /// Creates a new shipyard at a space station
    /// </summary>
    [HttpPost("shipyards")]
    public async Task<ActionResult<ShipyardDto>> CreateShipyard([FromBody] CreateShipyardRequest request)
    {
        try
        {
            var userId = GetUserId();
            var shipyard = await _spaceshipService.CreateShipyardAsync(userId, request);
            var dto = MapShipyardToDto(shipyard);
            return CreatedAtAction(nameof(GetGameShipyards), new { gameId = request.GameId }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all shipyards for the current player in a game
    /// </summary>
    [HttpGet("shipyards/game/{gameId}")]
    public async Task<ActionResult<List<ShipyardDto>>> GetGameShipyards(int gameId)
    {
        var userId = GetUserId();
        var shipyards = await _spaceshipService.GetPlayerShipyardsAsync(gameId, userId);
        var dtos = shipyards.Select(MapShipyardToDto).ToList();
        return Ok(dtos);
    }

    #endregion

    #region Spaceship CRUD Endpoints

    /// <summary>
    /// Creates a new spaceship at a shipyard
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SpaceshipDto>> CreateSpaceship([FromBody] CreateSpaceshipRequest request)
    {
        try
        {
            var userId = GetUserId();
            var spaceship = await _spaceshipService.CreateSpaceshipAsync(userId, request);
            var dto = MapSpaceshipToDto(spaceship);
            return CreatedAtAction(nameof(GetSpaceship), new { id = spaceship.Id }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all spaceships for the current player in a game
    /// </summary>
    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<List<SpaceshipDto>>> GetGameSpaceships(int gameId)
    {
        var userId = GetUserId();
        var spaceships = await _spaceshipService.GetPlayerSpaceshipsAsync(gameId, userId);
        var dtos = spaceships.Select(MapSpaceshipToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Gets a specific spaceship by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SpaceshipDto>> GetSpaceship(int id)
    {
        var userId = GetUserId();
        var spaceship = await _spaceshipService.GetSpaceshipAsync(id, userId);
        
        if (spaceship == null)
        {
            return NotFound(new { error = "Spaceship not found" });
        }

        var dto = MapSpaceshipToDto(spaceship);
        return Ok(dto);
    }

    /// <summary>
    /// Gets all spaceships in a star system
    /// </summary>
    [HttpGet("system/{systemId}")]
    public async Task<ActionResult<List<SpaceshipDto>>> GetSystemSpaceships(int systemId)
    {
        var spaceships = await _spaceshipService.GetSystemSpaceshipsAsync(systemId);
        var dtos = spaceships.Select(MapSpaceshipToDto).ToList();
        return Ok(dtos);
    }

    #endregion

    #region Ship Movement Endpoints

    /// <summary>
    /// Moves a spaceship to a destination
    /// </summary>
    [HttpPost("{id}/move")]
    public async Task<ActionResult<SpaceshipDto>> MoveSpaceship(int id, [FromBody] MoveSpaceshipRequest request)
    {
        try
        {
            request.SpaceshipId = id;
            var userId = GetUserId();
            var spaceship = await _spaceshipService.MoveSpaceshipAsync(userId, request);
            var dto = MapSpaceshipToDto(spaceship);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Colony Ship Endpoints

    /// <summary>
    /// Colonizes a planet with a colony ship
    /// </summary>
    [HttpPost("{id}/colonize")]
    public async Task<ActionResult<object>> ColonizePlanet(int id, [FromBody] ColonizePlanetRequest request)
    {
        try
        {
            request.SpaceshipId = id;
            var userId = GetUserId();
            var planet = await _spaceshipService.ColonizePlanetAsync(userId, request);
            return Ok(new 
            { 
                success = true, 
                message = $"Planet {planet.Name} colonized successfully",
                planetId = planet.Id
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Mapping Methods

    private ShipyardDto MapShipyardToDto(Shipyard shipyard)
    {
        return new ShipyardDto
        {
            Id = shipyard.Id,
            PlayerId = shipyard.PlayerId,
            GameId = shipyard.GameId,
            SpaceStationId = shipyard.SpaceStationId,
            SpaceStationName = shipyard.SpaceStation?.Name ?? string.Empty,
            Name = shipyard.Name,
            MaxConcurrentBuilds = shipyard.MaxConcurrentBuilds,
            IsActive = shipyard.IsActive,
            CreatedAt = shipyard.CreatedAt,
            CurrentBuildsCount = shipyard.Spaceships?
                .Count(s => s.State == ShipState.UnderConstruction) ?? 0
        };
    }

    private SpaceshipDto MapSpaceshipToDto(Spaceship spaceship)
    {
        return new SpaceshipDto
        {
            Id = spaceship.Id,
            PlayerId = spaceship.PlayerId,
            GameId = spaceship.GameId,
            Name = spaceship.Name,
            Type = spaceship.Type.ToString(),
            State = spaceship.State.ToString(),
            ShipyardId = spaceship.ShipyardId,
            ConstructionProgress = spaceship.ConstructionProgress,
            ConstructionTimeSeconds = spaceship.ConstructionTimeSeconds,
            ConstructionStartTime = spaceship.ConstructionStartTime,
            ConstructionCompletedTime = spaceship.ConstructionCompletedTime,
            CurrentSystemId = spaceship.CurrentSystemId,
            CurrentSystemName = spaceship.CurrentSystem?.Name,
            PositionX = spaceship.PositionX,
            PositionY = spaceship.PositionY,
            Speed = spaceship.Speed,
            DestinationSystemId = spaceship.DestinationSystemId,
            DestinationSystemName = spaceship.DestinationSystem?.Name,
            DestinationX = spaceship.DestinationX,
            DestinationY = spaceship.DestinationY,
            MovementStartTime = spaceship.MovementStartTime,
            EstimatedArrivalTime = spaceship.EstimatedArrivalTime,
            CargoIron = spaceship.CargoIron,
            CargoCopper = spaceship.CargoCopper,
            CargoFuel = spaceship.CargoFuel,
            CargoSoil = spaceship.CargoSoil,
            CargoCapacity = spaceship.CargoCapacity,
            Health = spaceship.Health,
            MaxHealth = spaceship.MaxHealth,
            Attack = spaceship.Attack,
            Defense = spaceship.Defense,
            CreatedAt = spaceship.CreatedAt,
            LastUpdatedAt = spaceship.LastUpdatedAt
        };
    }

    #endregion
}
