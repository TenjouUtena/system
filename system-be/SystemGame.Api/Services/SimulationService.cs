using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Services.Agents;

namespace SystemGame.Api.Services;

public class SimulationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SimulationService> _logger;
    private readonly AgentExecutionService? _agentExecutionService;
    private readonly SpaceshipService? _spaceshipService;
    private static readonly Random _random = new Random();

    public SimulationService(
        ApplicationDbContext context,
        ILogger<SimulationService> logger,
        AgentExecutionService? agentExecutionService = null,
        SpaceshipService? spaceshipService = null)
    {
        _context = context;
        _logger = logger;
        _agentExecutionService = agentExecutionService;
        _spaceshipService = spaceshipService;
    }

    public async Task ProcessTickAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.Id == gameId && g.IsActive, cancellationToken);

            if (game == null)
            {
                return;
            }

            // Process construction progress
            await ProcessConstructionAsync();

            // Process resource production
            await ProcessResourceProductionAsync(gameId);

            // Process spaceship construction (Phase 7)
            if (_spaceshipService != null)
            {
                await _spaceshipService.ProcessShipConstructionAsync(gameId);
            }

            // Process spaceship movement (Phase 7)
            if (_spaceshipService != null)
            {
                await _spaceshipService.ProcessShipMovementAsync(gameId);
            }

            // Process agents (Phase 6)
            if (_agentExecutionService != null)
            {
                await _agentExecutionService.ProcessAgentsAsync(gameId, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tick for game {GameId}", gameId);
        }
    }

    private async Task ProcessConstructionAsync()
    {
        var buildingsInProgress = await _context.Buildings
            .Include(b => b.GridSquare)
            .Where(b => !b.IsComplete && b.ConstructionProgress < 100)
            .ToListAsync();

        foreach (var building in buildingsInProgress)
        {
            // Calculate elapsed time since construction start
            if (building.ConstructionStartTime.HasValue)
            {
                var elapsed = DateTime.UtcNow - building.ConstructionStartTime.Value;
                
                // Building construction time: 5 minutes (300 seconds)
                var constructionTimeSeconds = 300;
                var progressPerSecond = 100.0 / constructionTimeSeconds;
                var newProgress = Math.Min(100, (int)(elapsed.TotalSeconds * progressPerSecond));

                building.ConstructionProgress = newProgress;

                if (building.ConstructionProgress >= 100)
                {
                    building.ConstructionProgress = 100;
                    building.IsComplete = true;
                    building.ConstructionCompleteTime = DateTime.UtcNow;

                    // Free up the builder
                    var builder = await _context.Builders
                        .FirstOrDefaultAsync(b => b.AssignedBuildingId == building.Id);
                    
                    if (builder != null)
                    {
                        builder.AssignedBuildingId = null;
                        builder.IsAvailable = true;
                    }

                    _logger.LogInformation("Building {BuildingId} completed", building.Id);
                }
            }
        }
    }

    private async Task ProcessResourceProductionAsync(int gameId)
    {
        // Get all completed buildings
        var completedBuildings = await _context.Buildings
            .Include(b => b.Player)
            .Include(b => b.GridSquare)
                .ThenInclude(g => g!.PlanetGrid)
            .Where(b => b.IsComplete)
            .ToListAsync();

        foreach (var building in completedBuildings)
        {
            // Get the grid square this building is on
            var gridSquare = building.GridSquare;
            
            if (gridSquare == null) continue;

            // Get the planet grid
            var planetGrid = await _context.PlanetGrids
                .Include(pg => pg.Planet)
                    .ThenInclude(p => p.StarSystem)
                        .ThenInclude(s => s.Galaxy)
                .FirstOrDefaultAsync(pg => pg.Id == gridSquare.PlanetGridId);

            if (planetGrid?.Planet.StarSystem.Galaxy.GameId != gameId) continue;

            // Get or create a space station for this player in this system
            var spaceStation = await _context.SpaceStations
                .FirstOrDefaultAsync(ss => 
                    ss.SystemId == planetGrid.Planet.StarSystem.Id && 
                    ss.PlayerId == building.PlayerId);

            if (spaceStation == null)
            {
                // Create a space station for the player
                spaceStation = new SpaceStation
                {
                    SystemId = planetGrid.Planet.StarSystem.Id,
                    PlayerId = building.PlayerId,
                    Name = $"{building.Player.DisplayName}'s Station",
                    CreatedAt = DateTime.UtcNow
                };
                _context.SpaceStations.Add(spaceStation);
                await _context.SaveChangesAsync();
            }

            // Produce resources based on building type
            double productionAmount = 0.1; // Production per tick (every 5 seconds)

            switch (building.Type)
            {
                case BuildingType.IronMiner:
                    if (gridSquare.IronAmount.HasValue && gridSquare.IronAmount.Value > 0)
                    {
                        productionAmount = Math.Min(productionAmount, gridSquare.IronAmount.Value);
                        gridSquare.IronAmount -= productionAmount;
                        spaceStation.IronAmount += productionAmount;
                    }
                    break;

                case BuildingType.CopperMiner:
                    if (gridSquare.CopperAmount.HasValue && gridSquare.CopperAmount.Value > 0)
                    {
                        productionAmount = Math.Min(productionAmount, gridSquare.CopperAmount.Value);
                        gridSquare.CopperAmount -= productionAmount;
                        spaceStation.CopperAmount += productionAmount;
                    }
                    break;

                case BuildingType.FuelMiner:
                    if (gridSquare.FuelAmount.HasValue && gridSquare.FuelAmount.Value > 0)
                    {
                        productionAmount = Math.Min(productionAmount, gridSquare.FuelAmount.Value);
                        gridSquare.FuelAmount -= productionAmount;
                        spaceStation.FuelAmount += productionAmount;
                    }
                    break;

                case BuildingType.Farm:
                    if (gridSquare.SoilAmount.HasValue && gridSquare.SoilAmount.Value > 0)
                    {
                        productionAmount = Math.Min(productionAmount, gridSquare.SoilAmount.Value);
                        gridSquare.SoilAmount -= productionAmount;
                        spaceStation.SoilAmount += productionAmount;
                    }
                    break;
            }
        }
    }
}

