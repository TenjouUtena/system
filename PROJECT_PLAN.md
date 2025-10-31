# System Game - Project Plan

## Project Overview

System is a multiplayer, real-time, long-term 4X strategy game where the universe persists and evolves continuously, even when players are offline. Players manage multiple star systems, colonize planets, extract resources, construct buildings and spaceships, and expand their empires across the galaxy.

### Core Game Mechanics

- **Real-time Simulation**: Game runs continuously in the background, processing construction, resource extraction, and other actions even when players are offline
- **Multiple Game Instances**: Each galaxy is a separate game that can run independently for days or weeks
- **Procedural Generation**: Galaxies are procedurally generated with connected star systems via wormholes
- **Resource Management**: Extract and manage resources (Iron, Copper, Fuel, Soil) across planet grids
- **Building & Construction**: Build miners, farms, and other structures on planet surfaces
- **Fleet Management**: Construct and command spaceships for exploration and combat
- **Agent-Based AI**: Extensible agent system for autonomous entity behaviors

### Technology Stack

- **Frontend**: Next.js 16 (TypeScript), Tailwind CSS 4, React 19
- **Backend**: C# .NET 9, Entity Framework Core
- **Database**: PostgreSQL
- **Cache/Pub-Sub**: Redis
- **Real-time Communication**: SignalR
- **Deployment**: Railway (planned)

---

## Phase Breakdown

### Phase 1: Core Infrastructure & Authentication ✅ COMPLETE

**Goal**: Establish foundation with database, authentication, and basic project structure

**Completed Deliverables**:
- [x] .NET 9 Web API project with Entity Framework and PostgreSQL
- [x] ASP.NET Core Identity with JWT authentication
- [x] Refresh token mechanism with Redis storage
- [x] OAuth2 support (Google configured)
- [x] Next.js frontend with TypeScript and Tailwind CSS
- [x] Authentication UI (login/register pages)
- [x] Protected routes and auto token refresh
- [x] Dashboard with basic navigation
- [x] Swagger/OpenAPI documentation
- [x] CORS configuration

**Testing**: Local and OAuth flows validated, builds succeed

---

### Phase 2: Galaxy & System Generation ✅ COMPLETE

**Goal**: Create game instances with procedurally generated galaxies

**Completed Deliverables**:
- [x] Game data models (Game, Galaxy, StarSystem, Wormhole, Planet, PlayerGame)
- [x] Procedural galaxy generation algorithm
- [x] Minimum spanning tree for guaranteed system connectivity
- [x] Planet generation (1-4 per system, varying sizes and types)
- [x] Galaxy map visualization (interactive SVG)
- [x] Game lobby UI with create/join functionality
- [x] API endpoints for game management and galaxy data
- [x] Database migration with proper relationships

**Key Technical Achievements**:
- Minimum spanning tree ensures all systems are reachable
- Spatial distribution algorithm prevents overcrowded maps
- Interactive map with zoom, pan, and click-to-navigate
- Multi-game support with isolated instances

---

### Phase 3: Planet Grid & Resource Generation (Next)

**Goal**: Generate planet surfaces with realistic resource distribution

**Planned Deliverables**:
- [ ] GridSquare entity for planet tiles
- [ ] Resource system (Iron, Copper, Fuel, Soil with extensible structure)
- [ ] Perlin noise or similar for realistic resource clustering
- [ ] Resource visualization on planet surface
- [ ] Grid square details panel
- [ ] Planet surface view with zoom controls
- [ ] Resource quantity calculations

**Technical Requirements**:
- Grid size = planet.Size × 20 (e.g., size 5 = 100×100)
- Efficient data storage for large grids
- Smooth rendering and interaction for large grids
- Realistic resource deposits (patches, veins, clusters)

---

### Phase 4: Building System & Construction

**Goal**: Players can place and construct buildings on planets

**Planned Deliverables**:
- [ ] Building entity with types (Miner, Farm, etc.)
- [ ] Building placement mechanics
- [ ] Builder entity for construction
- [ ] Construction progress tracking (0-100%)
- [ ] Space station entity
- [ ] Space elevator concept
- [ ] Building management UI

**Technical Requirements**:
- Time-based construction
- One building per grid square
- Visual feedback during construction
- Storage and resource handling

---

### Phase 5: Real-Time Background Simulation Engine

**Goal**: Continuous game state processing independent of player connections

**Planned Deliverables**:
- [ ] Background worker service
- [ ] Simulation tick system (1-5 second intervals)
- [ ] Batch processing for efficiency
- [ ] State persistence (Redis + PostgreSQL)
- [ ] SignalR real-time updates
- [ ] Offline progress calculation

**Technical Requirements**:
- Horizontal scalability
- Efficient tick processing for large games
- Reliable state synchronization
- Graceful error handling

---

### Phase 6: Extensible Agent Behavior System

**Goal**: Framework for programming entity AI behaviors

**Planned Deliverables**:
- [ ] Agent entity and base interface
- [ ] Behavior plugin architecture
- [ ] Configuration-driven behaviors (JSON/config)
- [ ] State machine for agent behaviors
- [ ] Basic behaviors (Builder, Miner, etc.)
- [ ] LLM integration hooks for future AI
- [ ] Agent execution in simulation loop

**Design Principles**:
- Highly extensible and pluggable
- Support programmatic behaviors
- Ready for LLM-based decision making
- JSON configuration support

---

### Phase 7: Spaceships & Shipyards

**Goal**: Build and manage spaceship fleets

**Planned Deliverables**:
- [ ] Spaceship entity with types (Capital, Cruiser, Destroyer, Carrier)
- [ ] Shipyard entity for construction
- [ ] Ship construction queue
- [ ] Ship movement in system space
- [ ] Wormhole travel mechanics
- [ ] Colony ship for planet colonization
- [ ] Fleet management UI

**Technical Requirements**:
- Position tracking in system coordinates
- Movement calculations and routing
- Resource consumption for construction
- Colony ship deployment mechanics

---

### Phase 8: Advanced Agent Behaviors & Fleet Management

**Goal**: Complex autonomous behaviors and automation

**Planned Deliverables**:
- [ ] Patrol route behaviors
- [ ] Resource ferry automation
- [ ] Exploration behaviors
- [ ] Fleet grouping and formations
- [ ] Order queue system
- [ ] Fleet management UI
- [ ] Patrol route editor

**Advanced Features**:
- Multi-step behavior sequences
- Conditional logic in behaviors
- Formation flying
- Automated supply chains

---

### Phase 9: Combat System Foundation

**Goal**: Implement ship-to-ship combat mechanics (NPCs initially)

**Planned Deliverables**:
- [ ] Ship combat attributes (weapons, armor, shields, health)
- [ ] Combat resolution algorithm
- [ ] NPC/pirate ships
- [ ] Battle reports and logs
- [ ] Combat visualization
- [ ] Ship loadout UI

**Technical Requirements**:
- Balanced damage calculations
- Ship destruction mechanics
- Battle replay/logging
- Visual combat feedback

---

### Phase 10: Multiplayer Preparation & PvP Foundation

**Goal**: Enable player interaction and competitive features

**Planned Deliverables**:
- [ ] Diplomatic relations system
- [ ] Territory/ownership tracking
- [ ] Fog of war and visibility mechanics
- [ ] PvP combat enablement
- [ ] Leaderboards and statistics
- [ ] Player interaction UI

**PvP Features**:
- Visibility based on sensors/scouting
- Territory conflicts
- Diplomatic options (alliances, treaties)
- Statistics tracking

---

## Technical Architecture

### Data Model

```
User (ASP.NET Identity)
  └─ PlayerGame (Junction)
       └─ Game ── Galaxy ── StarSystem ── Planet ── GridSquare
                                         └─ Wormhole (connects systems)
  └─ Agent (Spaceship, Builder, etc.)
```

### Key Design Principles

1. **Real-time Continuity**: Game state updates continuously, not only on player action
2. **Extensibility**: Plugin-based architecture for behaviors, resources, building types
3. **Scalability**: Horizontal scaling ready with Redis pub/sub
4. **Performance**: Efficient batch processing and caching strategies
5. **Type Safety**: Full TypeScript and C# type safety throughout

### Infrastructure Patterns

- **Background Jobs**: Worker service for simulation ticks
- **Event-Driven**: SignalR for real-time updates
- **Caching Strategy**: Redis for hot data, PostgreSQL for persistence
- **Migration Strategy**: EF Core migrations with version control
- **API Design**: RESTful with consistent DTO patterns

---

## Development Workflow

### Branch Strategy

- `main`: Stable, deployable code
- Feature branches for each phase
- Pull request reviews before merging

### Testing Strategy

- Unit tests for core logic (planned)
- Integration tests for API endpoints (planned)
- End-to-end testing for user flows
- Performance testing for large galaxies

### Deployment Strategy

- Development: Local with Docker Compose
- Staging: Railway staging environment
- Production: Railway production with monitoring

---

## Risk Mitigation

### Technical Risks

1. **Performance at Scale**: Large galaxy generation and simulation
   - Mitigation: Batch processing, spatial indexing, caching strategies

2. **Data Consistency**: Real-time updates vs. offline calculations
   - Mitigation: Event sourcing, conflict resolution strategies

3. **Complex Agent Behaviors**: Extensibility without chaos
   - Mitigation: Clear interfaces, sandboxed execution, testing framework

### Project Risks

1. **Scope Creep**: Ambitious feature set
   - Mitigation: Strict phase completion criteria, MVP focus

2. **Technical Debt**: Rapid prototyping
   - Mitigation: Regular refactoring, code reviews, documentation

---

## Success Criteria

### Phase Completion Criteria

Each phase must:
- All deliverables implemented and tested
- No critical bugs or lint errors
- Documentation updated
- Ready for integration with next phase

### Project Success Criteria

- Players can create and join games
- Galaxies generate correctly with connectivity
- Resources can be extracted and managed
- Buildings construct over time
- Ships can move and travel between systems
- Agents execute behaviors autonomously
- Multiple players can interact in the same game
- System performs well with 50+ players and 100+ systems

---

## Timeline Estimates

- Phase 1: ✅ Complete
- Phase 2: ✅ Complete
- Phase 3: 1-2 weeks
- Phase 4: 1-2 weeks
- Phase 5: 2-3 weeks
- Phase 6: 2-3 weeks
- Phase 7: 1-2 weeks
- Phase 8: 1-2 weeks
- Phase 9: 1-2 weeks
- Phase 10: 2-3 weeks

**Total Estimated Timeline**: 12-20 weeks for full implementation

---

## Future Enhancements (Post-MVP)

- 3D galaxy visualization
- Mobile client (React Native)
- Advanced combat tactics and formations
- Economic systems (trading, markets)
- Research and technology trees
- Diplomacy and alliances
- Player-guild systems
- Spectator mode
- Replay system
- AI opponents
- Modding support

---

**Document Version**: 1.0  
**Last Updated**: Phase 2 Completion  
**Status**: In Progress - Ready for Phase 3

