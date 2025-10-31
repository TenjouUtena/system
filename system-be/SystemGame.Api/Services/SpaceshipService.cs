using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;

namespace SystemGame.Api.Services;

/// <summary>
/// Service for managing spaceships and shipyards
/// </summary>
public class SpaceshipService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SpaceshipService> _logger;

    public SpaceshipService(ApplicationDbContext context, ILogger<SpaceshipService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Shipyard Management

    /// <summary>
    /// Creates a new shipyard at a space station
    /// </summary>
    public async Task<Shipyard> CreateShipyardAsync(string playerId, CreateShipyardRequest request)
    {
        // Verify space station exists and belongs to player
        var spaceStation = await _context.SpaceStations
            .FirstOrDefaultAsync(ss => ss.Id == request.SpaceStationId && ss.PlayerId == playerId);

        if (spaceStation == null)
        {
            throw new InvalidOperationException("Space station not found or does not belong to player");
        }

        var shipyard = new Shipyard
        {
            PlayerId = playerId,
            GameId = request.GameId,
            SpaceStationId = request.SpaceStationId,
            Name = request.Name,
            MaxConcurrentBuilds = request.MaxConcurrentBuilds,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shipyards.Add(shipyard);
        await _context.SaveChangesAsync();

        return shipyard;
    }

    /// <summary>
    /// Gets all shipyards for a player in a game
    /// </summary>
    public async Task<List<Shipyard>> GetPlayerShipyardsAsync(int gameId, string playerId)
    {
        return await _context.Shipyards
            .Include(sy => sy.SpaceStation)
            .Where(sy => sy.GameId == gameId && sy.PlayerId == playerId)
            .ToListAsync();
    }

    #endregion

    #region Spaceship Management

    /// <summary>
    /// Creates a new spaceship and starts construction
    /// </summary>
    public async Task<Spaceship> CreateSpaceshipAsync(string playerId, CreateSpaceshipRequest request)
    {
        // Verify shipyard exists and belongs to player
        var shipyard = await _context.Shipyards
            .Include(sy => sy.SpaceStation)
                .ThenInclude(ss => ss.StarSystem)
            .FirstOrDefaultAsync(sy => sy.Id == request.ShipyardId && sy.PlayerId == playerId);

        if (shipyard == null)
        {
            throw new InvalidOperationException("Shipyard not found or does not belong to player");
        }

        // Check if shipyard has capacity
        var currentBuilds = await _context.Spaceships
            .CountAsync(s => s.ShipyardId == request.ShipyardId && s.State == ShipState.UnderConstruction);

        if (currentBuilds >= shipyard.MaxConcurrentBuilds)
        {
            throw new InvalidOperationException("Shipyard is at maximum capacity");
        }

        // Parse ship type
        if (!Enum.TryParse<ShipType>(request.Type, out var shipType))
        {
            throw new ArgumentException("Invalid ship type");
        }

        // Get ship stats based on type
        var (constructionTime, speed, cargoCapacity, health, attack, defense) = GetShipStats(shipType);

        var spaceship = new Spaceship
        {
            PlayerId = playerId,
            GameId = request.GameId,
            Name = request.Name,
            Type = shipType,
            State = ShipState.UnderConstruction,
            ShipyardId = request.ShipyardId,
            ConstructionProgress = 0,
            ConstructionTimeSeconds = constructionTime,
            ConstructionStartTime = DateTime.UtcNow,
            CurrentSystemId = shipyard.SpaceStation.SystemId,
            PositionX = 0, // Start at center of system
            PositionY = 0,
            Speed = speed,
            CargoCapacity = cargoCapacity,
            Health = health,
            MaxHealth = health,
            Attack = attack,
            Defense = defense,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.Spaceships.Add(spaceship);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created spaceship {ShipName} of type {ShipType} at shipyard {ShipyardId}", 
            spaceship.Name, spaceship.Type, request.ShipyardId);

        return spaceship;
    }

    /// <summary>
    /// Gets ship statistics based on type
    /// </summary>
    private (int constructionTime, double speed, int cargoCapacity, int health, int attack, int defense) GetShipStats(ShipType type)
    {
        return type switch
        {
            ShipType.Scout => (60, 20.0, 10, 50, 5, 5),           // 1 minute, fast
            ShipType.Colony => (300, 5.0, 50, 100, 0, 10),        // 5 minutes, slow
            ShipType.Freighter => (180, 8.0, 1000, 150, 0, 15),   // 3 minutes, large cargo
            ShipType.Destroyer => (240, 12.0, 50, 200, 50, 30),   // 4 minutes
            ShipType.Cruiser => (420, 10.0, 100, 300, 80, 50),    // 7 minutes
            ShipType.Carrier => (600, 8.0, 200, 400, 60, 60),     // 10 minutes
            ShipType.Capital => (900, 6.0, 300, 600, 100, 100),   // 15 minutes
            _ => (300, 10.0, 100, 100, 10, 10)
        };
    }

    /// <summary>
    /// Gets all spaceships for a player in a game
    /// </summary>
    public async Task<List<Spaceship>> GetPlayerSpaceshipsAsync(int gameId, string playerId)
    {
        return await _context.Spaceships
            .Include(s => s.CurrentSystem)
            .Include(s => s.DestinationSystem)
            .Include(s => s.Shipyard)
            .Where(s => s.GameId == gameId && s.PlayerId == playerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific spaceship by ID
    /// </summary>
    public async Task<Spaceship?> GetSpaceshipAsync(int spaceshipId, string playerId)
    {
        return await _context.Spaceships
            .Include(s => s.CurrentSystem)
            .Include(s => s.DestinationSystem)
            .Include(s => s.Shipyard)
            .FirstOrDefaultAsync(s => s.Id == spaceshipId && s.PlayerId == playerId);
    }

    /// <summary>
    /// Gets all spaceships in a star system
    /// </summary>
    public async Task<List<Spaceship>> GetSystemSpaceshipsAsync(int systemId)
    {
        return await _context.Spaceships
            .Include(s => s.Player)
            .Where(s => s.CurrentSystemId == systemId && s.State != ShipState.UnderConstruction)
            .ToListAsync();
    }

    #endregion

    #region Ship Movement

    /// <summary>
    /// Initiates ship movement to a destination
    /// </summary>
    public async Task<Spaceship> MoveSpaceshipAsync(string playerId, MoveSpaceshipRequest request)
    {
        var spaceship = await _context.Spaceships
            .Include(s => s.CurrentSystem)
            .FirstOrDefaultAsync(s => s.Id == request.SpaceshipId && s.PlayerId == playerId);

        if (spaceship == null)
        {
            throw new InvalidOperationException("Spaceship not found or does not belong to player");
        }

        if (spaceship.State == ShipState.UnderConstruction)
        {
            throw new InvalidOperationException("Cannot move ship under construction");
        }

        if (spaceship.State == ShipState.Moving)
        {
            throw new InvalidOperationException("Ship is already moving");
        }

        // If moving to a different system, verify wormhole connection exists
        if (request.DestinationSystemId.HasValue && request.DestinationSystemId.Value != spaceship.CurrentSystemId)
        {
            var wormholeExists = await _context.Wormholes
                .AnyAsync(w => 
                    (w.SystemAId == spaceship.CurrentSystemId && w.SystemBId == request.DestinationSystemId) ||
                    (w.SystemBId == spaceship.CurrentSystemId && w.SystemAId == request.DestinationSystemId));

            if (!wormholeExists)
            {
                throw new InvalidOperationException("No wormhole connection exists to destination system");
            }
        }

        // Calculate distance and estimated arrival time
        double distance;
        if (request.DestinationSystemId.HasValue && request.DestinationSystemId.Value != spaceship.CurrentSystemId)
        {
            // Moving to different system - fixed wormhole travel time (30 seconds)
            distance = 300; // Fixed distance for wormhole travel
        }
        else
        {
            // Moving within same system
            var dx = request.DestinationX - spaceship.PositionX;
            var dy = request.DestinationY - spaceship.PositionY;
            distance = Math.Sqrt(dx * dx + dy * dy);
        }

        var travelTimeSeconds = distance / spaceship.Speed;
        var estimatedArrival = DateTime.UtcNow.AddSeconds(travelTimeSeconds);

        spaceship.State = ShipState.Moving;
        spaceship.DestinationSystemId = request.DestinationSystemId;
        spaceship.DestinationX = request.DestinationX;
        spaceship.DestinationY = request.DestinationY;
        spaceship.MovementStartTime = DateTime.UtcNow;
        spaceship.EstimatedArrivalTime = estimatedArrival;
        spaceship.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ship {ShipId} started moving from ({X1},{Y1}) to ({X2},{Y2})", 
            spaceship.Id, spaceship.PositionX, spaceship.PositionY, request.DestinationX, request.DestinationY);

        return spaceship;
    }

    /// <summary>
    /// Processes ship movement during simulation tick
    /// </summary>
    public async Task ProcessShipMovementAsync(int gameId)
    {
        var movingShips = await _context.Spaceships
            .Where(s => s.GameId == gameId && s.State == ShipState.Moving)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var ship in movingShips)
        {
            if (ship.EstimatedArrivalTime.HasValue && now >= ship.EstimatedArrivalTime.Value)
            {
                // Ship has arrived
                if (ship.DestinationSystemId.HasValue && ship.DestinationSystemId.Value != ship.CurrentSystemId)
                {
                    // Moved to different system
                    ship.CurrentSystemId = ship.DestinationSystemId.Value;
                }

                ship.PositionX = ship.DestinationX ?? ship.PositionX;
                ship.PositionY = ship.DestinationY ?? ship.PositionY;
                ship.State = ShipState.Idle;
                ship.DestinationSystemId = null;
                ship.DestinationX = null;
                ship.DestinationY = null;
                ship.MovementStartTime = null;
                ship.EstimatedArrivalTime = null;
                ship.LastUpdatedAt = now;

                _logger.LogInformation("Ship {ShipId} arrived at destination", ship.Id);
            }
        }

        if (movingShips.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Ship Construction

    /// <summary>
    /// Processes ship construction during simulation tick
    /// </summary>
    public async Task ProcessShipConstructionAsync(int gameId)
    {
        var constructingShips = await _context.Spaceships
            .Where(s => s.GameId == gameId && s.State == ShipState.UnderConstruction)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var ship in constructingShips)
        {
            if (!ship.ConstructionStartTime.HasValue)
                continue;

            var elapsedSeconds = (now - ship.ConstructionStartTime.Value).TotalSeconds;
            var progress = (elapsedSeconds / ship.ConstructionTimeSeconds) * 100;

            if (progress >= 100)
            {
                // Construction complete
                ship.ConstructionProgress = 100;
                ship.State = ShipState.Idle;
                ship.ConstructionCompletedTime = now;
                ship.LastUpdatedAt = now;

                _logger.LogInformation("Ship {ShipName} construction completed", ship.Name);
            }
            else
            {
                ship.ConstructionProgress = progress;
                ship.LastUpdatedAt = now;
            }
        }

        if (constructingShips.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Colony Ships

    /// <summary>
    /// Colonizes a planet with a colony ship
    /// </summary>
    public async Task<Planet> ColonizePlanetAsync(string playerId, ColonizePlanetRequest request)
    {
        var ship = await _context.Spaceships
            .Include(s => s.CurrentSystem)
                .ThenInclude(s => s.Planets)
            .FirstOrDefaultAsync(s => s.Id == request.SpaceshipId && s.PlayerId == playerId);

        if (ship == null)
        {
            throw new InvalidOperationException("Spaceship not found or does not belong to player");
        }

        if (ship.Type != ShipType.Colony)
        {
            throw new InvalidOperationException("Only colony ships can colonize planets");
        }

        if (ship.State != ShipState.Idle)
        {
            throw new InvalidOperationException("Ship must be idle to colonize");
        }

        var planet = await _context.Planets
            .FirstOrDefaultAsync(p => p.Id == request.PlanetId && p.SystemId == ship.CurrentSystemId);

        if (planet == null)
        {
            throw new InvalidOperationException("Planet not found in ship's current system");
        }

        // Check if planet is already colonized
        var existingStation = await _context.SpaceStations
            .AnyAsync(ss => ss.SystemId == planet.SystemId && ss.PlayerId == playerId);

        if (!existingStation)
        {
            // Create space station for the player in this system
            var spaceStation = new SpaceStation
            {
                SystemId = planet.SystemId,
                PlayerId = playerId,
                Name = $"{planet.Name} Station",
                CreatedAt = DateTime.UtcNow
            };

            _context.SpaceStations.Add(spaceStation);
        }

        // Create a builder on the planet
        var builder = new Builder
        {
            PlanetId = planet.Id,
            PlayerId = playerId,
            Name = $"Builder {planet.Name}",
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Builders.Add(builder);

        // Destroy the colony ship
        ship.State = ShipState.Destroyed;
        ship.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Planet {PlanetId} colonized by player {PlayerId}, colony ship destroyed", 
            planet.Id, playerId);

        return planet;
    }

    #endregion
}
