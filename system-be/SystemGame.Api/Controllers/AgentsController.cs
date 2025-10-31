using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SystemGame.Api.Contexts;
using SystemGame.Api.Models;
using SystemGame.Api.Services.Agents;

namespace SystemGame.Api.Controllers;

[ApiController]
[Route("api/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AgentExecutionService _executionService;
    private readonly AgentBehaviorService _behaviorService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(
        ApplicationDbContext context,
        AgentExecutionService executionService,
        AgentBehaviorService behaviorService,
        ILogger<AgentsController> logger)
    {
        _context = context;
        _executionService = executionService;
        _behaviorService = behaviorService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    // GET /api/agents/game/{gameId}
    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<List<AgentDto>>> GetGameAgents(int gameId)
    {
        var userId = GetUserId();

        // Verify user is in the game
        var playerGame = await _context.PlayerGames
            .FirstOrDefaultAsync(pg => pg.GameId == gameId && pg.UserId == userId);

        if (playerGame == null)
        {
            return Forbid();
        }

        var agents = await _context.Agents
            .Include(a => a.CurrentSystem)
            .Include(a => a.CurrentPlanet)
            .Include(a => a.Builder)
            .Where(a => a.GameId == gameId && a.PlayerId == userId)
            .Select(a => new AgentDto
            {
                Id = a.Id,
                PlayerId = a.PlayerId,
                GameId = a.GameId,
                Type = a.Type.ToString(),
                Name = a.Name,
                State = a.State.ToString(),
                CurrentBehaviorName = a.CurrentBehaviorName,
                BehaviorConfig = a.BehaviorConfig,
                LastExecutionTime = a.LastExecutionTime,
                CreatedAt = a.CreatedAt,
                CurrentSystemId = a.CurrentSystemId,
                CurrentSystemName = a.CurrentSystem != null ? a.CurrentSystem.Name : null,
                CurrentPlanetId = a.CurrentPlanetId,
                CurrentPlanetName = a.CurrentPlanet != null ? a.CurrentPlanet.Name : null,
                BuilderId = a.BuilderId,
                BuilderName = a.Builder != null ? a.Builder.Name : null
            })
            .ToListAsync();

        return Ok(agents);
    }

    // GET /api/agents/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetAgent(int id)
    {
        var userId = GetUserId();

        var agent = await _context.Agents
            .Include(a => a.CurrentSystem)
            .Include(a => a.CurrentPlanet)
            .Include(a => a.Builder)
            .Where(a => a.Id == id && a.PlayerId == userId)
            .Select(a => new AgentDto
            {
                Id = a.Id,
                PlayerId = a.PlayerId,
                GameId = a.GameId,
                Type = a.Type.ToString(),
                Name = a.Name,
                State = a.State.ToString(),
                CurrentBehaviorName = a.CurrentBehaviorName,
                BehaviorConfig = a.BehaviorConfig,
                LastExecutionTime = a.LastExecutionTime,
                CreatedAt = a.CreatedAt,
                CurrentSystemId = a.CurrentSystemId,
                CurrentSystemName = a.CurrentSystem != null ? a.CurrentSystem.Name : null,
                CurrentPlanetId = a.CurrentPlanetId,
                CurrentPlanetName = a.CurrentPlanet != null ? a.CurrentPlanet.Name : null,
                BuilderId = a.BuilderId,
                BuilderName = a.Builder != null ? a.Builder.Name : null
            })
            .FirstOrDefaultAsync();

        if (agent == null)
        {
            return NotFound();
        }

        return Ok(agent);
    }

    // POST /api/agents
    [HttpPost]
    public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] CreateAgentRequest request)
    {
        try
        {
            var userId = GetUserId();
            var agent = await _executionService.CreateAgentAsync(request, userId);

            var dto = new AgentDto
            {
                Id = agent.Id,
                PlayerId = agent.PlayerId,
                GameId = agent.GameId,
                Type = agent.Type.ToString(),
                Name = agent.Name,
                State = agent.State.ToString(),
                CurrentBehaviorName = agent.CurrentBehaviorName,
                BehaviorConfig = agent.BehaviorConfig,
                LastExecutionTime = agent.LastExecutionTime,
                CreatedAt = agent.CreatedAt,
                BuilderId = agent.BuilderId
            };

            return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT /api/agents/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<AgentDto>> UpdateAgent(int id, [FromBody] UpdateAgentRequest request)
    {
        try
        {
            var userId = GetUserId();
            var agent = await _executionService.UpdateAgentAsync(id, request, userId);

            var dto = new AgentDto
            {
                Id = agent.Id,
                PlayerId = agent.PlayerId,
                GameId = agent.GameId,
                Type = agent.Type.ToString(),
                Name = agent.Name,
                State = agent.State.ToString(),
                CurrentBehaviorName = agent.CurrentBehaviorName,
                BehaviorConfig = agent.BehaviorConfig,
                LastExecutionTime = agent.LastExecutionTime,
                CreatedAt = agent.CreatedAt,
                BuilderId = agent.BuilderId
            };

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // POST /api/agents/{id}/pause
    [HttpPost("{id}/pause")]
    public async Task<IActionResult> PauseAgent(int id)
    {
        try
        {
            var userId = GetUserId();
            await _executionService.PauseAgentAsync(id, userId);
            return Ok(new { message = "Agent paused successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // POST /api/agents/{id}/resume
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> ResumeAgent(int id)
    {
        try
        {
            var userId = GetUserId();
            await _executionService.ResumeAgentAsync(id, userId);
            return Ok(new { message = "Agent resumed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // DELETE /api/agents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        try
        {
            var userId = GetUserId();
            await _executionService.DeleteAgentAsync(id, userId);
            return Ok(new { message = "Agent deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // GET /api/agents/{id}/logs
    [HttpGet("{id}/logs")]
    public async Task<ActionResult<List<AgentLogDto>>> GetAgentLogs(int id, [FromQuery] int limit = 100)
    {
        try
        {
            var userId = GetUserId();
            var logs = await _executionService.GetAgentLogsAsync(id, userId, limit);

            var dtos = logs.Select(l => new AgentLogDto
            {
                Id = l.Id,
                Timestamp = l.Timestamp,
                Level = l.Level.ToString(),
                Message = l.Message,
                Data = l.Data
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // GET /api/agents/behaviors
    [HttpGet("behaviors")]
    public ActionResult<List<BehaviorInfoDto>> GetAvailableBehaviors()
    {
        var behaviors = _behaviorService.GetAllBehaviors()
            .Select(b => new BehaviorInfoDto
            {
                Name = b.Name,
                Description = b.Description,
                SupportedAgentTypes = b.SupportedAgentTypes.Select(t => t.ToString()).ToList()
            })
            .ToList();

        return Ok(behaviors);
    }

    // POST /api/agents/{id}/test-behavior
    [HttpPost("{id}/test-behavior")]
    public async Task<ActionResult<object>> TestBehavior(int id)
    {
        try
        {
            var userId = GetUserId();
            
            var agent = await _context.Agents
                .Include(a => a.Game)
                .FirstOrDefaultAsync(a => a.Id == id && a.PlayerId == userId);

            if (agent == null)
            {
                return NotFound(new { error = "Agent not found" });
            }

            var context = new BehaviorContext
            {
                DbContext = _context,
                Logger = _logger,
                Game = agent.Game,
                SharedData = new Dictionary<string, object>(),
                CancellationToken = default
            };

            var result = await _behaviorService.ExecuteBehaviorAsync(agent, context);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                nextState = result.NextState.ToString(),
                logData = result.LogData
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
