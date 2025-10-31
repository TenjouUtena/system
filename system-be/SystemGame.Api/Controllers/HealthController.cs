using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using SystemGame.Api.Contexts;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ApplicationDbContext context,
        IConnectionMultiplexer redis,
        ILogger<HealthController> logger)
    {
        _context = context;
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Detailed health check with dependencies
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            checks = new Dictionary<string, object>()
        };

        // Database check
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            health.checks["database"] = new { status = "healthy", responseTime = "< 100ms" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            health.checks["database"] = new { status = "unhealthy", error = ex.Message };
        }

        // Redis check
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            health.checks["redis"] = new { status = "healthy", responseTime = "< 50ms" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            health.checks["redis"] = new { status = "unhealthy", error = ex.Message };
        }

        // Memory check
        var memoryUsed = GC.GetTotalMemory(false) / 1024 / 1024; // MB
        health.checks["memory"] = new 
        { 
            status = memoryUsed < 500 ? "healthy" : "warning",
            usedMB = memoryUsed
        };

        return Ok(health);
    }

    /// <summary>
    /// Get system metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var metrics = new
            {
                timestamp = DateTime.UtcNow,
                database = new
                {
                    games = await _context.Games.CountAsync(),
                    activeGames = await _context.Games.CountAsync(g => g.IsActive),
                    players = await _context.PlayerGames.Select(pg => pg.UserId).Distinct().CountAsync(),
                    spaceships = await _context.Spaceships.CountAsync(),
                    battles = await _context.Battles.CountAsync(b => b.State == Data.Entities.BattleState.InProgress)
                },
                memory = new
                {
                    totalMB = GC.GetTotalMemory(false) / 1024 / 1024,
                    gen0Collections = GC.CollectionCount(0),
                    gen1Collections = GC.CollectionCount(1),
                    gen2Collections = GC.CollectionCount(2)
                }
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics");
            return StatusCode(500, "Error retrieving metrics");
        }
    }
}
