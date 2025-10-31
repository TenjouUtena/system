# Phase 6: Extensible Agent Behavior System

## Overview

Phase 6 introduces an **extensible agent behavior system** that allows entities in the game to act autonomously based on configurable behaviors. This system is designed to be highly pluggable, supporting both programmatic behaviors and future LLM-based decision making.

### Goals

1. Create a flexible agent framework that can control various game entities
2. Implement a pluggable behavior system with clear interfaces
3. Support configuration-driven behaviors (JSON/config)
4. Integrate agents into the existing simulation loop
5. Provide basic behaviors for common tasks (building automation, resource transport)
6. Prepare infrastructure for future LLM integration
7. Create UI for agent management and monitoring

---

## Data Model

### Core Entities

#### Agent Entity
```csharp
public class Agent
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public AgentType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public AgentState State { get; set; } = AgentState.Idle;
    public string? CurrentBehaviorName { get; set; }
    public string? BehaviorConfig { get; set; } // JSON configuration
    public DateTime LastExecutionTime { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Location tracking (nullable - not all agents have locations)
    public int? CurrentSystemId { get; set; }
    public int? CurrentPlanetId { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    
    // Entity references (polymorphic)
    public int? BuilderId { get; set; }
    public int? SpaceshipId { get; set; } // Future: Phase 7
    
    // Navigation properties
    public virtual AppUser Player { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
    public virtual StarSystem? CurrentSystem { get; set; }
    public virtual Planet? CurrentPlanet { get; set; }
    public virtual Builder? Builder { get; set; }
    public virtual ICollection<AgentLog> Logs { get; set; } = new List<AgentLog>();
}
```

#### AgentType Enum
```csharp
public enum AgentType
{
    Builder,        // Construction automation
    ResourceFerry,  // Transport resources between locations
    Scout,          // Exploration (future)
    Fleet,          // Ship group management (future)
    Custom          // User-defined or LLM-driven
}
```

#### AgentState Enum
```csharp
public enum AgentState
{
    Idle,           // Waiting for task
    Active,         // Currently executing behavior
    Paused,         // Temporarily paused by player
    Error,          // Encountered an error
    Completed       // Finished assigned tasks
}
```

#### AgentLog Entity
```csharp
public class AgentLog
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; } // JSON for structured data
    
    // Navigation
    public virtual Agent Agent { get; set; } = null!;
}
```

---

## Behavior System Architecture

### Core Interfaces

#### IAgentBehavior Interface
```csharp
public interface IAgentBehavior
{
    string Name { get; }
    string Description { get; }
    AgentType[] SupportedAgentTypes { get; }
    
    Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context);
    Task<bool> CanExecuteAsync(Agent agent, BehaviorContext context);
    Task ValidateConfigAsync(string? config);
}
```

#### BehaviorContext Class
```csharp
public class BehaviorContext
{
    public ApplicationDbContext DbContext { get; set; } = null!;
    public ILogger Logger { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public Dictionary<string, object> SharedData { get; set; } = new();
    public CancellationToken CancellationToken { get; set; }
}
```

#### BehaviorResult Class
```csharp
public class BehaviorResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AgentState NextState { get; set; } = AgentState.Active;
    public string? LogData { get; set; } // JSON for logging
    public TimeSpan? NextExecutionDelay { get; set; } // For throttling
}
```

---

## Built-in Behaviors

### 1. AutoBuilderBehavior

**Purpose**: Automatically assigns builders to construct queued buildings

**Configuration**:
```json
{
  "planetIds": [1, 2, 3],
  "priorityBuildingTypes": ["IronMiner", "CopperMiner"],
  "maxConcurrentBuildings": 3
}
```

**Logic**:
1. Check if builder is available
2. Find incomplete buildings on configured planets
3. Prioritize by type configuration
4. Assign builder to highest priority building
5. Log assignment

### 2. ResourceFerryBehavior

**Purpose**: Transports resources between space stations

**Configuration**:
```json
{
  "sourceSystemId": 1,
  "targetSystemId": 2,
  "resourceType": "Iron",
  "minAmount": 100,
  "maxAmount": 500,
  "ferryInterval": 300 // seconds
}
```

**Logic**:
1. Check if enough resources at source
2. Calculate ferry amount
3. Deduct from source station
4. Add to target station
5. Log transaction

### 3. ProductionMonitorBehavior

**Purpose**: Monitors resource production and sends alerts

**Configuration**:
```json
{
  "monitoredResources": ["Iron", "Copper"],
  "alertThresholds": {
    "Iron": 1000,
    "Copper": 500
  },
  "checkInterval": 60
}
```

**Logic**:
1. Check all player space stations
2. Compare against thresholds
3. Create log entries for low resources
4. Update shared data for UI alerts

### 4. IdleBehavior

**Purpose**: Default behavior when no other behavior is assigned

**Logic**:
1. Log idle state
2. Return success with longer delay

---

## Service Layer

### AgentBehaviorService

**Responsibilities**:
- Register and manage behaviors
- Execute agent behaviors
- Handle behavior lifecycle
- Validate configurations

```csharp
public class AgentBehaviorService
{
    private readonly Dictionary<string, IAgentBehavior> _behaviors;
    private readonly ILogger<AgentBehaviorService> _logger;

    public void RegisterBehavior(IAgentBehavior behavior);
    public IAgentBehavior? GetBehavior(string name);
    public IEnumerable<IAgentBehavior> GetBehaviorsForType(AgentType type);
    public Task<BehaviorResult> ExecuteBehaviorAsync(Agent agent, BehaviorContext context);
}
```

### AgentExecutionService

**Responsibilities**:
- Process all agents in simulation loop
- Manage agent state transitions
- Handle errors and logging
- Throttle execution based on delays

```csharp
public class AgentExecutionService
{
    public async Task ProcessAgentsAsync(int gameId);
    public async Task<Agent> CreateAgentAsync(CreateAgentRequest request);
    public async Task<Agent> UpdateAgentAsync(int agentId, UpdateAgentRequest request);
    public async Task PauseAgentAsync(int agentId);
    public async Task ResumeAgentAsync(int agentId);
    public async Task DeleteAgentAsync(int agentId);
    public async Task<List<AgentLog>> GetAgentLogsAsync(int agentId, int limit = 100);
}
```

---

## Simulation Integration

### Update SimulationService

Add agent processing to existing simulation loop:

```csharp
public async Task ProcessTickAsync(int gameId)
{
    try
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(g => g.Id == gameId && g.IsActive);

        if (game == null) return;

        // Existing systems
        await ProcessConstructionAsync();
        await ProcessResourceProductionAsync(gameId);
        
        // NEW: Process agents
        await _agentExecutionService.ProcessAgentsAsync(gameId);

        await _context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing tick for game {GameId}", gameId);
    }
}
```

---

## API Endpoints

### AgentsController

```csharp
[ApiController]
[Route("api/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    // GET /api/agents/game/{gameId}
    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<List<AgentDto>>> GetGameAgents(int gameId);
    
    // GET /api/agents/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetAgent(int id);
    
    // POST /api/agents
    [HttpPost]
    public async Task<ActionResult<AgentDto>> CreateAgent(CreateAgentRequest request);
    
    // PUT /api/agents/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<AgentDto>> UpdateAgent(int id, UpdateAgentRequest request);
    
    // POST /api/agents/{id}/pause
    [HttpPost("{id}/pause")]
    public async Task<IActionResult> PauseAgent(int id);
    
    // POST /api/agents/{id}/resume
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> ResumeAgent(int id);
    
    // DELETE /api/agents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(int id);
    
    // GET /api/agents/{id}/logs
    [HttpGet("{id}/logs")]
    public async Task<ActionResult<List<AgentLogDto>>> GetAgentLogs(int id, [FromQuery] int limit = 100);
    
    // GET /api/agents/behaviors
    [HttpGet("behaviors")]
    public async Task<ActionResult<List<BehaviorInfoDto>>> GetAvailableBehaviors();
    
    // POST /api/agents/{id}/test-behavior
    [HttpPost("{id}/test-behavior")]
    public async Task<ActionResult<BehaviorResult>> TestBehavior(int id);
}
```

---

## DTOs and Models

### AgentDto
```csharp
public class AgentDto
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? CurrentBehaviorName { get; set; }
    public string? BehaviorConfig { get; set; }
    public DateTime LastExecutionTime { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Location
    public int? CurrentSystemId { get; set; }
    public string? CurrentSystemName { get; set; }
    public int? CurrentPlanetId { get; set; }
    public string? CurrentPlanetName { get; set; }
    
    // Entity references
    public int? BuilderId { get; set; }
    public string? BuilderName { get; set; }
}
```

### CreateAgentRequest
```csharp
public class CreateAgentRequest
{
    public int GameId { get; set; }
    public string Type { get; set; } = string.Empty; // AgentType
    public string Name { get; set; } = string.Empty;
    public string? BehaviorName { get; set; }
    public string? BehaviorConfig { get; set; } // JSON
    public int? BuilderId { get; set; }
}
```

### UpdateAgentRequest
```csharp
public class UpdateAgentRequest
{
    public string? Name { get; set; }
    public string? BehaviorName { get; set; }
    public string? BehaviorConfig { get; set; }
}
```

### AgentLogDto
```csharp
public class AgentLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
}
```

### BehaviorInfoDto
```csharp
public class BehaviorInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportedAgentTypes { get; set; } = new();
    public string? ConfigSchema { get; set; } // JSON Schema for validation
}
```

---

## Frontend Implementation

### New Components

#### 1. AgentList Component (`app/games/[id]/agents/page.tsx`)

**Features**:
- List all agents in the game
- Filter by type and state
- Create new agent button
- Quick actions (pause/resume/delete)

#### 2. AgentDetail Component (`app/games/[id]/agents/[agentId]/page.tsx`)

**Features**:
- Agent details and configuration
- Real-time status updates via SignalR
- Behavior configuration editor (JSON)
- Activity log viewer
- Performance metrics

#### 3. CreateAgentModal Component

**Features**:
- Agent type selection
- Name input
- Behavior selection dropdown
- JSON config editor with validation
- Entity binding (select builder, etc.)

#### 4. AgentLogViewer Component

**Features**:
- Real-time log streaming
- Filter by log level
- Export logs
- Search functionality

### API Client Updates

```typescript
// lib/api/agent.ts
export interface Agent {
  id: number;
  playerId: string;
  gameId: number;
  type: string;
  name: string;
  state: string;
  currentBehaviorName?: string;
  behaviorConfig?: string;
  lastExecutionTime: string;
  createdAt: string;
  currentSystemId?: number;
  currentSystemName?: string;
  currentPlanetId?: number;
  currentPlanetName?: string;
  builderId?: number;
  builderName?: string;
}

export interface CreateAgentRequest {
  gameId: number;
  type: string;
  name: string;
  behaviorName?: string;
  behaviorConfig?: string;
  builderId?: number;
}

export interface AgentLog {
  id: number;
  timestamp: string;
  level: string;
  message: string;
  data?: string;
}

export interface BehaviorInfo {
  name: string;
  description: string;
  supportedAgentTypes: string[];
  configSchema?: string;
}

export const agentApi = {
  getGameAgents: (gameId: number) => 
    apiClient.get<Agent[]>(`/api/agents/game/${gameId}`),
  
  getAgent: (id: number) => 
    apiClient.get<Agent>(`/api/agents/${id}`),
  
  createAgent: (request: CreateAgentRequest) => 
    apiClient.post<Agent>('/api/agents', request),
  
  updateAgent: (id: number, request: Partial<CreateAgentRequest>) => 
    apiClient.put<Agent>(`/api/agents/${id}`, request),
  
  pauseAgent: (id: number) => 
    apiClient.post(`/api/agents/${id}/pause`),
  
  resumeAgent: (id: number) => 
    apiClient.post(`/api/agents/${id}/resume`),
  
  deleteAgent: (id: number) => 
    apiClient.delete(`/api/agents/${id}`),
  
  getAgentLogs: (id: number, limit: number = 100) => 
    apiClient.get<AgentLog[]>(`/api/agents/${id}/logs?limit=${limit}`),
  
  getAvailableBehaviors: () => 
    apiClient.get<BehaviorInfo[]>('/api/agents/behaviors'),
  
  testBehavior: (id: number) => 
    apiClient.post<any>(`/api/agents/${id}/test-behavior`)
};
```

### SignalR Integration

Update `GameHub` to broadcast agent events:

```csharp
public class GameHub : Hub
{
    // Existing methods...
    
    public async Task SendAgentUpdate(int gameId, AgentDto agent)
    {
        await Clients.Group($"game-{gameId}").SendAsync("AgentUpdated", agent);
    }
    
    public async Task SendAgentLog(int gameId, int agentId, AgentLogDto log)
    {
        await Clients.Group($"game-{gameId}").SendAsync("AgentLogReceived", agentId, log);
    }
}
```

Frontend SignalR listener:

```typescript
// In agent detail page
useEffect(() => {
  const connection = new HubConnectionBuilder()
    .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/hubs/game`)
    .build();

  connection.on('AgentUpdated', (agent: Agent) => {
    if (agent.id === agentId) {
      setAgentData(agent);
    }
  });

  connection.on('AgentLogReceived', (receivedAgentId: number, log: AgentLog) => {
    if (receivedAgentId === agentId) {
      setLogs(prev => [log, ...prev]);
    }
  });

  connection.start();

  return () => {
    connection.stop();
  };
}, [agentId]);
```

---

## Future Extensibility: LLM Integration

### Design for LLM Behaviors

The system is designed to support LLM-based behaviors in future phases:

#### LLMBehavior Base Class
```csharp
public abstract class LLMBehaviorBase : IAgentBehavior
{
    protected readonly ILLMService _llmService;
    
    public async Task<BehaviorResult> ExecuteAsync(Agent agent, BehaviorContext context)
    {
        // 1. Gather context (game state, agent state, etc.)
        var gameContext = await BuildGameContextAsync(agent, context);
        
        // 2. Call LLM for decision
        var decision = await _llmService.GetDecisionAsync(agent, gameContext);
        
        // 3. Validate and execute decision
        return await ExecuteDecisionAsync(agent, decision, context);
    }
    
    protected abstract Task<string> BuildGameContextAsync(Agent agent, BehaviorContext context);
    protected abstract Task<BehaviorResult> ExecuteDecisionAsync(Agent agent, string decision, BehaviorContext context);
}
```

#### Configuration for LLM Behaviors
```json
{
  "behaviorType": "LLM",
  "model": "gpt-4",
  "systemPrompt": "You are a strategic builder agent...",
  "constraints": {
    "maxResourcesPerAction": 1000,
    "allowedActions": ["build", "ferry", "scout"]
  },
  "executionInterval": 60
}
```

---

## Database Migration

### Migration: Phase6AgentSystem

**Creates**:
- `Agents` table with all properties
- `AgentLogs` table for logging
- Foreign keys to Games, Players, Builders, Systems, Planets
- Indexes on GameId, PlayerId, State for performance
- Check constraints on State and Type enums

**SQL Summary**:
```sql
CREATE TABLE Agents (
    Id INT PRIMARY KEY IDENTITY,
    PlayerId NVARCHAR(450) NOT NULL,
    GameId INT NOT NULL,
    Type INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    State INT NOT NULL DEFAULT 0,
    CurrentBehaviorName NVARCHAR(100),
    BehaviorConfig NVARCHAR(MAX),
    LastExecutionTime DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CurrentSystemId INT NULL,
    CurrentPlanetId INT NULL,
    PositionX FLOAT NULL,
    PositionY FLOAT NULL,
    BuilderId INT NULL,
    SpaceshipId INT NULL,
    CONSTRAINT FK_Agents_Players FOREIGN KEY (PlayerId) REFERENCES AspNetUsers(Id),
    CONSTRAINT FK_Agents_Games FOREIGN KEY (GameId) REFERENCES Games(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Agents_Systems FOREIGN KEY (CurrentSystemId) REFERENCES StarSystems(Id),
    CONSTRAINT FK_Agents_Planets FOREIGN KEY (CurrentPlanetId) REFERENCES Planets(Id),
    CONSTRAINT FK_Agents_Builders FOREIGN KEY (BuilderId) REFERENCES Builders(Id)
);

CREATE TABLE AgentLogs (
    Id INT PRIMARY KEY IDENTITY,
    AgentId INT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    Level INT NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    Data NVARCHAR(MAX),
    CONSTRAINT FK_AgentLogs_Agents FOREIGN KEY (AgentId) REFERENCES Agents(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Agents_GameId ON Agents(GameId);
CREATE INDEX IX_Agents_PlayerId ON Agents(PlayerId);
CREATE INDEX IX_Agents_State ON Agents(State);
CREATE INDEX IX_AgentLogs_AgentId_Timestamp ON AgentLogs(AgentId, Timestamp DESC);
```

---

## Testing Plan

### Unit Tests

1. **Behavior Tests**
   - Test each behavior's logic in isolation
   - Mock database context
   - Verify correct state transitions
   - Test error handling

2. **Agent Execution Tests**
   - Test agent state management
   - Test behavior selection
   - Test error recovery
   - Test throttling

3. **Configuration Tests**
   - Test JSON parsing
   - Test validation
   - Test schema enforcement

### Integration Tests

1. **Simulation Loop**
   - Create test agents
   - Run simulation tick
   - Verify behaviors execute
   - Verify logs created

2. **API Endpoints**
   - Test CRUD operations
   - Test authorization
   - Test invalid inputs
   - Test SignalR updates

3. **End-to-End**
   - Create agent via UI
   - Configure behavior
   - Observe execution
   - View logs
   - Pause/resume/delete

---

## Performance Considerations

### Optimization Strategies

1. **Batch Processing**
   - Process multiple agents per tick
   - Use single DbContext per game
   - Minimize database round-trips

2. **Throttling**
   - Agents can specify minimum execution interval
   - Skip agents that executed recently
   - Prioritize active agents

3. **Caching**
   - Cache behavior instances
   - Cache game state for read-heavy behaviors
   - Use Redis for shared data between agents

4. **Parallel Execution**
   - Process independent agents in parallel
   - Use Task.WhenAll for I/O operations
   - Ensure thread-safe behavior implementations

### Performance Targets

- Process 100 agents in < 500ms per tick
- Support 1000+ agents per game
- Agent creation < 100ms
- Log query < 50ms for 1000 logs

---

## Security Considerations

### Agent Access Control

1. **Ownership Verification**
   - Only owners can modify agents
   - Read access for game members
   - Admin override for moderation

2. **Behavior Restrictions**
   - Validate all configurations
   - Sanitize JSON inputs
   - Rate limit behavior changes
   - Prevent resource exploits

3. **Execution Sandboxing**
   - Isolate behavior execution
   - Timeout long-running behaviors
   - Catch and log all exceptions
   - Prevent cross-agent interference

---

## Documentation Requirements

### Developer Documentation

1. **Behavior Development Guide**
   - How to create custom behaviors
   - Interface requirements
   - Testing strategies
   - Best practices

2. **Configuration Schema**
   - JSON schema for each behavior
   - Example configurations
   - Validation rules

3. **API Documentation**
   - Endpoint descriptions
   - Request/response examples
   - Error codes
   - Rate limits

### User Documentation

1. **Agent Setup Guide**
   - How to create agents
   - Behavior selection
   - Configuration examples
   - Troubleshooting

2. **Behavior Reference**
   - Description of each behavior
   - Use cases
   - Configuration options
   - Expected outcomes

---

## Deliverables Checklist

### Backend ✅

- [ ] Agent entity and migration
- [ ] AgentLog entity and migration
- [ ] IAgentBehavior interface
- [ ] BehaviorContext and BehaviorResult classes
- [ ] AgentBehaviorService implementation
- [ ] AgentExecutionService implementation
- [ ] Built-in behaviors:
  - [ ] AutoBuilderBehavior
  - [ ] ResourceFerryBehavior
  - [ ] ProductionMonitorBehavior
  - [ ] IdleBehavior
- [ ] AgentsController with all endpoints
- [ ] Update SimulationService to process agents
- [ ] Update GameHub for agent events
- [ ] Unit tests for behaviors
- [ ] Integration tests for agent system

### Frontend ✅

- [ ] Agent types and interfaces
- [ ] Agent API client
- [ ] AgentList page component
- [ ] AgentDetail page component
- [ ] CreateAgentModal component
- [ ] AgentLogViewer component
- [ ] SignalR integration for real-time updates
- [ ] Behavior configuration editor
- [ ] Agent status indicators

### Documentation ✅

- [ ] Phase6_Complete.md (after implementation)
- [ ] API documentation updates
- [ ] Behavior development guide
- [ ] User guide for agents
- [ ] Update PROJECT_STATUS.md
- [ ] Update PROJECT_PLAN.md

---

## Timeline Estimate

- **Week 1**: Backend entities, migrations, and core interfaces (2-3 days)
- **Week 1-2**: Behavior system implementation and built-in behaviors (3-4 days)
- **Week 2**: Service layer and simulation integration (2-3 days)
- **Week 2-3**: API endpoints and SignalR updates (2-3 days)
- **Week 3**: Frontend components and UI (3-4 days)
- **Week 3**: Testing and bug fixes (2-3 days)
- **Week 3-4**: Documentation and polish (1-2 days)

**Total Estimated Time**: 2-3 weeks

---

## Success Criteria

Phase 6 is complete when:

1. ✅ Agents can be created, updated, paused, resumed, and deleted
2. ✅ At least 3 built-in behaviors are fully implemented
3. ✅ Behaviors execute correctly in simulation loop
4. ✅ Agent logs are created and viewable
5. ✅ Frontend UI for agent management is functional
6. ✅ Real-time updates via SignalR work correctly
7. ✅ Configuration validation prevents invalid setups
8. ✅ All tests pass
9. ✅ Documentation is complete
10. ✅ System supports extensibility for custom behaviors

---

## Next Phase Preview: Phase 7

Phase 7 will build on the agent system to add:
- **Spaceship entity** for fleet management
- **Shipyard buildings** for ship construction
- **Ship movement** and wormhole travel
- **Colony ships** for planet colonization
- **Fleet agent behaviors** for ship automation
- Integration of ships with agent system

The agent system created in Phase 6 will provide the foundation for autonomous fleet management in Phase 7.

---

**Document Version**: 1.0  
**Status**: Ready for Implementation  
**Dependencies**: Phase 1-5 Complete  
**Target Completion**: 2-3 weeks from start
