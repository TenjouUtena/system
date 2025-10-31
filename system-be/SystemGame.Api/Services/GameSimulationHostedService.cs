using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;

namespace SystemGame.Api.Services;

public class GameSimulationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameSimulationHostedService> _logger;

    public GameSimulationHostedService(
        IServiceProvider serviceProvider,
        ILogger<GameSimulationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait 10 seconds on startup before processing
        await Task.Delay(10000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var simulationService = scope.ServiceProvider.GetRequiredService<SimulationService>();

                // Get all active games
                var games = await context.Games
                    .Where(g => g.IsActive)
                    .ToListAsync(stoppingToken);

                // Process each game
                foreach (var game in games)
                {
                    await simulationService.ProcessTickAsync(game.Id);
                }

                _logger.LogDebug("Processed {Count} games", games.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simulation tick");
            }

            // Wait 5 seconds before next tick
            await Task.Delay(5000, stoppingToken);
        }
    }
}

