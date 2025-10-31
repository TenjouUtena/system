using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services;

public class GalaxyGeneratorService
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random;

    public GalaxyGeneratorService(ApplicationDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<Galaxy> GenerateGalaxyAsync(int gameId, string name, int systemCount = 20, PlanetGridGeneratorService? planetGridGenerator = null)
    {
        var galaxy = new Galaxy
        {
            GameId = gameId,
            Name = name,
            SystemCount = systemCount,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Galaxies.AddAsync(galaxy);
        await _context.SaveChangesAsync();

        // Generate systems
        var systems = await GenerateSystemsAsync(galaxy.Id, systemCount);

        // Generate wormholes to ensure connectivity
        await GenerateWormholesAsync(systems);

        // Generate planets for each system
        foreach (var system in systems)
        {
            await GeneratePlanetsAsync(system, planetGridGenerator);
        }

        await _context.SaveChangesAsync();
        return galaxy;
    }

    private async Task<List<StarSystem>> GenerateSystemsAsync(int galaxyId, int count)
    {
        var systems = new List<StarSystem>();
        var minDistance = 50; // Minimum distance between systems
        var usedPositions = new HashSet<(int X, int Y)>();

        for (int i = 0; i < count; i++)
        {
            var system = new StarSystem
            {
                GalaxyId = galaxyId,
                Name = GenerateSystemName(),
                X = 0,
                Y = 0
            };

            // Find a valid position
            int attempts = 0;
            do
            {
                system.X = _random.Next(-1000, 1001);
                system.Y = _random.Next(-1000, 1001);
                attempts++;

                // If too many attempts, relax constraints
                if (attempts > 1000)
                {
                    minDistance = Math.Max(10, minDistance / 2);
                    attempts = 0;
                }
            } while (usedPositions.Any(p => 
                Math.Sqrt(Math.Pow(system.X - p.X, 2) + Math.Pow(system.Y - p.Y, 2)) < minDistance) &&
                attempts < 2000);

            usedPositions.Add((system.X, system.Y));
            systems.Add(system);
        }

        await _context.StarSystems.AddRangeAsync(systems);
        await _context.SaveChangesAsync();
        return systems;
    }

    private async Task GenerateWormholesAsync(List<StarSystem> systems)
    {
        var wormholes = new List<Wormhole>();

        // Create a minimum spanning tree to ensure all systems are connected
        var mst = CreateMinimumSpanningTree(systems);
        foreach (var edge in mst)
        {
            wormholes.Add(new Wormhole
            {
                SystemAId = edge.Item1.Id,
                SystemBId = edge.Item2.Id,
                IsActive = true
            });
        }

        // Add additional random wormholes (1-4 per system)
        var additionalConnections = new HashSet<string>();
        foreach (var system in systems)
        {
            var existingConnections = mst.Count(e => 
                e.Item1.Id == system.Id || e.Item2.Id == system.Id);
            var targetConnections = _random.Next(1, 5);

            for (int i = existingConnections; i < targetConnections; i++)
            {
                var otherSystem = systems[_random.Next(systems.Count)];
                
                // Ensure different system and not already connected
                int attempts = 0;
                while ((otherSystem.Id == system.Id || 
                       additionalConnections.Contains($"{system.Id}_{otherSystem.Id}") ||
                       additionalConnections.Contains($"{otherSystem.Id}_{system.Id}")) &&
                       attempts < 100)
                {
                    otherSystem = systems[_random.Next(systems.Count)];
                    attempts++;
                }

                if (attempts < 100)
                {
                    additionalConnections.Add($"{system.Id}_{otherSystem.Id}");
                    wormholes.Add(new Wormhole
                    {
                        SystemAId = system.Id,
                        SystemBId = otherSystem.Id,
                        IsActive = true
                    });
                }
            }
        }

        await _context.Wormholes.AddRangeAsync(wormholes);
    }

    private List<(StarSystem, StarSystem)> CreateMinimumSpanningTree(List<StarSystem> systems)
    {
        if (systems.Count < 2) return new List<(StarSystem, StarSystem)>();

        var mst = new List<(StarSystem, StarSystem)>();
        var visited = new HashSet<int> { systems[0].Id };
        
        while (visited.Count < systems.Count)
        {
            var minWeight = double.MaxValue;
            StarSystem? nearestUnvisited = null;
            StarSystem? nearestVisited = null;

            foreach (var visitedSystem in systems.Where(s => visited.Contains(s.Id)))
            {
                foreach (var unvisitedSystem in systems.Where(s => !visited.Contains(s.Id)))
                {
                    var weight = Math.Sqrt(
                        Math.Pow(visitedSystem.X - unvisitedSystem.X, 2) +
                        Math.Pow(visitedSystem.Y - unvisitedSystem.Y, 2)
                    );

                    if (weight < minWeight)
                    {
                        minWeight = weight;
                        nearestVisited = visitedSystem;
                        nearestUnvisited = unvisitedSystem;
                    }
                }
            }

            if (nearestVisited != null && nearestUnvisited != null)
            {
                mst.Add((nearestVisited, nearestUnvisited));
                visited.Add(nearestUnvisited.Id);
            }
        }

        return mst;
    }

    private async Task GeneratePlanetsAsync(StarSystem system, PlanetGridGeneratorService? planetGridGenerator)
    {
        // Each system gets 1-4 planets
        var planetCount = _random.Next(1, 5);
        var planets = new List<Planet>();

        for (int i = 0; i < planetCount; i++)
        {
            var planet = new Planet
            {
                SystemId = system.Id,
                Name = $"{system.Name} {(char)('I' + i)}",
                Size = _random.Next(3, 9), // Size 3-8
                Type = (PlanetType)_random.Next(1, 5)
            };

            planets.Add(planet);
        }

        await _context.Planets.AddRangeAsync(planets);
        await _context.SaveChangesAsync();

        // Generate grids for all planets if grid generator is provided
        if (planetGridGenerator != null)
        {
            foreach (var planet in planets)
            {
                await planetGridGenerator.GenerateGridAsync(planet);
            }
        }
    }

    private string GenerateSystemName()
    {
        var prefixes = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa" };
        var suffixes = new[] { "Centauri", "Draconis", "Herculis", "Pegasi", "Vega", "Sirius", "Proxima", "Sol" };
        
        return $"{prefixes[_random.Next(prefixes.Length)]} {suffixes[_random.Next(suffixes.Length)]}";
    }
}

