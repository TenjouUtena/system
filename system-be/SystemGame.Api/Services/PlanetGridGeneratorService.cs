using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services;

public class PlanetGridGeneratorService
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random;

    public PlanetGridGeneratorService(ApplicationDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<PlanetGrid> GenerateGridAsync(Planet planet)
    {
        var gridSize = planet.Size * 20;
        
        var grid = new PlanetGrid
        {
            PlanetId = planet.Id,
            Width = gridSize,
            Height = gridSize,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PlanetGrids.AddAsync(grid);
        await _context.SaveChangesAsync();

        // Generate resource distributions based on planet type
        var resourceMap = GenerateResourceMap(gridSize, planet.Type);

        // Create grid squares
        var squares = new List<GridSquare>();
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                var resources = resourceMap[x, y];
                var square = new GridSquare
                {
                    PlanetGridId = grid.Id,
                    X = x,
                    Y = y,
                    IronAmount = resources.Iron,
                    CopperAmount = resources.Copper,
                    FuelAmount = resources.Fuel,
                    SoilAmount = resources.Soil
                };

                squares.Add(square);
            }
        }

        // Batch insert for performance
        await _context.GridSquares.AddRangeAsync(squares);
        await _context.SaveChangesAsync();

        return grid;
    }

    private (double? Iron, double? Copper, double? Fuel, double? Soil)[,] GenerateResourceMap(int size, PlanetType planetType)
    {
        var map = new (double?, double?, double?, double?)[size, size];
        
        // Generate base noise maps for each resource
        var ironNoise = GenerateNoiseMap(size, _random.Next(0, 10000));
        var copperNoise = GenerateNoiseMap(size, _random.Next(0, 10000));
        var fuelNoise = GenerateNoiseMap(size, _random.Next(0, 10000));
        var soilNoise = GenerateNoiseMap(size, _random.Next(0, 10000));

        // Configure resource spawn rates based on planet type
        var (ironRate, copperRate, fuelRate, soilRate) = GetResourceRatesForPlanetType(planetType);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                map[x, y] = (
                    GetResourceAmount(ironNoise, x, y, ironRate),
                    GetResourceAmount(copperNoise, x, y, copperRate),
                    GetResourceAmount(fuelNoise, x, y, fuelRate),
                    GetResourceAmount(soilNoise, x, y, soilRate)
                );
            }
        }

        return map;
    }

    private double[,] GenerateNoiseMap(int size, int seed)
    {
        var rng = new Random(seed);
        var noise = new double[size, size];

        // Generate base noise
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noise[x, y] = rng.NextDouble();
            }
        }

        // Apply simple smoothing/convolution for clustering
        var smoothed = new double[size, size];
        int kernelSize = Math.Min(5, size / 10 + 1);
        int radius = kernelSize / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                double sum = 0;
                int count = 0;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                        {
                            sum += noise[nx, ny];
                            count++;
                        }
                    }
                }

                smoothed[x, y] = sum / count;
            }
        }

        return smoothed;
    }

    private double? GetResourceAmount(double[,] noise, int x, int y, double spawnRate)
    {
        var value = noise[x, y];
        
        // Only spawn resources above threshold
        if (value < spawnRate)
        {
            return null;
        }

        // Scale the amount based on how far above threshold
        var excess = (value - spawnRate) / (1 - spawnRate);
        
        // Map to resource amount (10-1000 units)
        return 10 + excess * 990;
    }

    private (double iron, double copper, double fuel, double soil) GetResourceRatesForPlanetType(PlanetType type)
    {
        return type switch
        {
            PlanetType.Terrestrial => (0.3, 0.2, 0.1, 0.5),  // Good soil, moderate minerals
            PlanetType.GasGiant => (0.4, 0.3, 0.6, 0.0),    // Rich in fuel, metals, no soil
            PlanetType.Ice => (0.2, 0.15, 0.4, 0.1),        // Some fuel, few minerals
            PlanetType.Desert => (0.5, 0.4, 0.3, 0.1),      // Rich metals, poor soil
            _ => (0.3, 0.3, 0.3, 0.3)
        };
    }
}

