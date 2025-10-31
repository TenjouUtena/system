# System Game - Project Plan v2.0
## Replanned for Launchable Game

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
- **Deployment**: Railway

---

## Current Status: 70% Complete

**✅ Phases 1-7 Complete** (Core Game Foundation)
**⏭️ Phases 8-12 Remaining** (Combat, Polish, Launch)

---

## Phase Breakdown

### ✅ Phase 1: Core Infrastructure & Authentication (COMPLETE)

**Status**: Production-ready ✅

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

**Documentation**: Phase1_Complete.md

---

### ✅ Phase 2: Galaxy & System Generation (COMPLETE)

**Status**: Production-ready ✅

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

**Documentation**: Phase2_Complete.md

---

### ✅ Phase 3: Planet Grid & Resource Generation (COMPLETE)

**Status**: Production-ready ✅

**Completed Deliverables**:
- [x] GridSquare entity for planet tiles
- [x] Resource system (Iron, Copper, Fuel, Soil with extensible structure)
- [x] Noise-based realistic resource clustering
- [x] Resource visualization on planet surface
- [x] Grid square details panel
- [x] Planet surface view with zoom controls
- [x] Resource quantity calculations

**Technical Achievements**:
- Grid size = planet.Size × 20 (e.g., size 5 = 100×100)
- Efficient data storage for large grids
- Smooth rendering for 10,000+ grid squares
- Realistic resource deposits with planet type variations

**Documentation**: Phase3_Complete.md

---

### ✅ Phase 4: Building System & Construction (COMPLETE)

**Status**: Production-ready ✅

**Completed Deliverables**:
- [x] Building entity with types (IronMiner, CopperMiner, FuelMiner, Farm)
- [x] Building placement mechanics
- [x] Builder entity for construction
- [x] Construction progress tracking (0-100%)
- [x] Space station entity with resource storage
- [x] Building management UI

**Technical Achievements**:
- Time-based construction (5 minutes default)
- One building per grid square
- Visual feedback during construction
- Automatic space station creation

**Documentation**: Phase4_5_Complete.md

---

### ✅ Phase 5: Real-Time Background Simulation Engine (COMPLETE)

**Status**: Production-ready ✅

**Completed Deliverables**:
- [x] Background worker service
- [x] Simulation tick system (5-second intervals)
- [x] Batch processing for efficiency
- [x] State persistence (Redis + PostgreSQL)
- [x] SignalR real-time updates
- [x] Offline progress calculation
- [x] Resource production from buildings
- [x] Grid resource depletion

**Technical Achievements**:
- Horizontal scalability ready
- Efficient tick processing for large games
- Reliable state synchronization
- Graceful error handling
- Supports 100+ games simultaneously

**Documentation**: Phase4_5_Complete.md

---

### ✅ Phase 6: Extensible Agent Behavior System (COMPLETE)

**Status**: Production-ready ✅

**Completed Deliverables**:
- [x] Agent and AgentLog entities with full data model
- [x] IAgentBehavior interface and behavior plugin architecture
- [x] BehaviorContext and BehaviorResult classes
- [x] Configuration-driven behaviors with JSON validation
- [x] State machine for agent behaviors (Idle, Active, Paused, Error, Completed)
- [x] Built-in behaviors:
  - [x] AutoBuilderBehavior - Construction automation
  - [x] ResourceFerryBehavior - Resource transport
  - [x] ProductionMonitorBehavior - Resource monitoring
  - [x] IdleBehavior - Default behavior
- [x] AgentBehaviorService and AgentExecutionService
- [x] Full REST API and frontend UI for agent management
- [x] LLM integration hooks and base classes
- [x] Agent execution integrated in simulation loop
- [x] Real-time updates via SignalR

**Design Principles**:
- Highly extensible and pluggable
- Support programmatic behaviors
- Ready for LLM-based decision making
- JSON configuration with validation
- Performance optimized (100+ agents per game)

**Documentation**: Phase6_Complete.md, Phase6_Plan.md

---

### ✅ Phase 7: Spaceships & Shipyards (COMPLETE)

**Status**: Production-ready ✅

**Completed Deliverables**:
- [x] Spaceship entity with 7 ship types (Scout, Colony, Freighter, Destroyer, Cruiser, Carrier, Capital)
- [x] Shipyard entity for construction
- [x] Ship construction queue with progress tracking
- [x] Ship movement in system space
- [x] Wormhole travel mechanics
- [x] Colony ship for planet colonization
- [x] Fleet management UI
- [x] Ship stats (Health, Attack, Defense, Cargo)
- [x] Real-time construction and movement processing

**Technical Achievements**:
- Position tracking in system coordinates
- Movement calculations and routing
- Distance-based within-system travel
- Fixed-time wormhole travel (30s)
- Colony ship deployment creates builders
- Integrated with simulation engine

**Documentation**: Phase7_Complete.md

---

## Remaining Phases (For Launchable Game)

### 📋 Phase 8: Combat System & NPCs

**Goal**: Implement ship-to-ship combat with NPC pirate encounters

**Status**: Not Started ⏳  
**Timeline**: 1-2 weeks  
**Priority**: HIGH (Core Gameplay Feature)

#### Backend Deliverables
- [ ] Combat resolution algorithm
  - [ ] Damage calculation using Attack/Defense stats
  - [ ] Health tracking and ship destruction
  - [ ] Weapon types and effectiveness
- [ ] NPC/Pirate ship system
  - [ ] NPC spawn service
  - [ ] NPC behavior patterns (patrol, ambush)
  - [ ] Difficulty scaling
- [ ] Battle entity and logging
  - [ ] BattleReport entity for combat history
  - [ ] Combat event logging
  - [ ] Experience/rewards system
- [ ] Combat service integration
  - [ ] Automatic combat detection (proximity-based)
  - [ ] Combat resolution in simulation tick
  - [ ] Ship respawn mechanics

#### Frontend Deliverables
- [ ] Combat visualization on galaxy map
  - [ ] Battle indicators on ships
  - [ ] Combat animations/effects
- [ ] Battle report viewer
  - [ ] Combat log display
  - [ ] Damage breakdown
  - [ ] Battle replay (text-based)
- [ ] Ship loadout UI
  - [ ] View ship combat stats
  - [ ] Compare ship types
- [ ] NPC encounter notifications

#### API Endpoints
- `GET /api/combat/battles/game/{gameId}` - Get battle history
- `GET /api/combat/battles/{id}` - Get battle details
- `POST /api/combat/attack` - Initiate manual attack (optional)
- `GET /api/npcs/game/{gameId}` - List NPC ships in game
- `POST /api/npcs/spawn` - Admin endpoint for NPC spawning

#### Success Criteria
- Players can engage NPCs in combat
- Combat is balanced and fun
- Ships can be destroyed and rebuilt
- Battle reports are clear and informative
- Combat integrates with existing ship movement

---

### 📋 Phase 9: UI/UX Polish & User Onboarding

**Goal**: Make the game intuitive, beautiful, and easy to learn

**Status**: Not Started ⏳  
**Timeline**: 2 weeks  
**Priority**: CRITICAL (Launchability Requirement)

#### User Experience Improvements
- [ ] **Tutorial System**
  - [ ] First-time user walkthrough
  - [ ] Interactive guide for core mechanics
  - [ ] Tooltips for all UI elements
  - [ ] Help modal with game rules
- [ ] **UI Polish**
  - [ ] Consistent design system
  - [ ] Loading states for all async operations
  - [ ] Error messages that are user-friendly
  - [ ] Success notifications
  - [ ] Confirmation dialogs for destructive actions
- [ ] **Navigation Improvements**
  - [ ] Breadcrumb navigation
  - [ ] Quick navigation shortcuts
  - [ ] Recent locations history
  - [ ] Minimap for galaxy overview
- [ ] **Dashboard Redesign**
  - [ ] Game status summary
  - [ ] Resource overview across all systems
  - [ ] Fleet status summary
  - [ ] Recent activity feed
  - [ ] Quick actions panel

#### Visual Enhancements
- [ ] **Galaxy Map**
  - [ ] Better zoom/pan controls
  - [ ] System labels and info on hover
  - [ ] Resource indicators per system
  - [ ] Fleet position markers
  - [ ] Combat zone highlights
- [ ] **Planet Surface**
  - [ ] Resource density heatmap
  - [ ] Building upgrade indicators
  - [ ] Construction zones
  - [ ] Grid coordinate labels
- [ ] **Ship Management**
  - [ ] Fleet formation display
  - [ ] Ship health bars
  - [ ] Travel path visualization
  - [ ] Cargo indicators
- [ ] **Animations & Transitions**
  - [ ] Smooth page transitions
  - [ ] Construction progress animations
  - [ ] Ship movement animations
  - [ ] Combat effects

#### Accessibility
- [ ] Keyboard navigation
- [ ] Screen reader support (ARIA labels)
- [ ] Color blind friendly palette
- [ ] Adjustable text size
- [ ] Reduced motion option

#### Success Criteria
- New players can complete tutorial in < 10 minutes
- All core mechanics are documented with tooltips
- UI is consistent across all pages
- Loading states prevent confusion
- Game feels polished and professional

---

### 📋 Phase 10: Game Balance & Tuning

**Goal**: Balance game mechanics for fun, strategic gameplay

**Status**: Not Started ⏳  
**Timeline**: 1 week  
**Priority**: HIGH (Playability Requirement)

#### Balance Adjustments
- [ ] **Resource Rates**
  - [ ] Tune mining rates for balanced economy
  - [ ] Adjust resource spawn density
  - [ ] Balance resource consumption
- [ ] **Construction Times**
  - [ ] Building construction time tuning
  - [ ] Ship construction time balancing
  - [ ] Builder availability balance
- [ ] **Ship Balance**
  - [ ] Adjust ship stats (Health, Attack, Defense)
  - [ ] Balance ship costs and build times
  - [ ] Speed and cargo capacity tuning
- [ ] **Combat Balance**
  - [ ] Damage calculations
  - [ ] NPC difficulty scaling
  - [ ] Combat rewards tuning
- [ ] **Galaxy Generation**
  - [ ] System count recommendations
  - [ ] Wormhole connection density
  - [ ] Starting resource distribution

#### Game Pacing
- [ ] Early game progression (0-30 min)
- [ ] Mid game expansion (30 min - 2 hours)
- [ ] Late game strategy (2+ hours)
- [ ] Multi-day gameplay flow

#### Configuration System
- [ ] Admin panel for game settings
- [ ] Per-game configuration options
- [ ] Preset difficulty levels (Easy, Normal, Hard)
- [ ] Custom game rules

#### Playtesting
- [ ] Internal playtesting sessions
- [ ] Balance feedback collection
- [ ] Iteration on problem areas
- [ ] Documentation of optimal strategies

#### Success Criteria
- Games feel balanced and fair
- No obvious exploits or broken strategies
- Early, mid, and late game all engaging
- Different ship types have clear purposes
- Resources are meaningful but not too scarce

---

### 📋 Phase 11: Performance Optimization & Stability

**Goal**: Ensure smooth performance and reliability at scale

**Status**: Not Started ⏳  
**Timeline**: 1 week  
**Priority**: HIGH (Production Readiness)

#### Backend Optimization
- [ ] **Database Performance**
  - [ ] Index optimization for common queries
  - [ ] Query performance analysis
  - [ ] Connection pooling tuning
  - [ ] Implement pagination for large datasets
- [ ] **Simulation Engine**
  - [ ] Tick processing optimization
  - [ ] Batch operation improvements
  - [ ] Memory usage profiling
  - [ ] Parallel processing for independent games
- [ ] **API Performance**
  - [ ] Response caching strategy
  - [ ] API rate limiting
  - [ ] Request compression (gzip)
  - [ ] Static resource optimization
- [ ] **SignalR Optimization**
  - [ ] Connection scaling
  - [ ] Message batching
  - [ ] Backpressure handling

#### Frontend Optimization
- [ ] **React Performance**
  - [ ] Component memoization
  - [ ] Lazy loading for routes
  - [ ] Code splitting
  - [ ] Asset optimization
- [ ] **Data Fetching**
  - [ ] Implement caching (React Query or SWR)
  - [ ] Optimistic updates
  - [ ] Background data refresh
  - [ ] Infinite scroll for large lists
- [ ] **Bundle Size**
  - [ ] Analyze and reduce bundle size
  - [ ] Tree shaking optimization
  - [ ] Dynamic imports
  - [ ] Image optimization

#### Error Handling & Logging
- [ ] Structured logging with Serilog
- [ ] Error tracking (Sentry or similar)
- [ ] Health check endpoints
- [ ] Monitoring dashboard (Grafana or similar)
- [ ] Automated alerting

#### Load Testing
- [ ] Simulate 100+ concurrent users
- [ ] Stress test simulation engine
- [ ] Database load testing
- [ ] Redis performance testing
- [ ] Identify and fix bottlenecks

#### Success Criteria
- API response times < 200ms for 95th percentile
- Simulation can handle 500+ games
- Frontend loads in < 3 seconds
- No memory leaks in long-running processes
- Zero critical bugs in production
- Graceful degradation under load

---

### 📋 Phase 12: Testing, Documentation & Deployment

**Goal**: Comprehensive testing, docs, and production deployment

**Status**: Not Started ⏳  
**Timeline**: 2 weeks  
**Priority**: CRITICAL (Launch Requirement)

#### Testing
- [ ] **Backend Unit Tests**
  - [ ] Service layer tests
  - [ ] Business logic tests
  - [ ] Data model tests
  - [ ] 70%+ code coverage target
- [ ] **Backend Integration Tests**
  - [ ] API endpoint tests
  - [ ] Database integration tests
  - [ ] Authentication flow tests
  - [ ] SignalR hub tests
- [ ] **Frontend Tests**
  - [ ] Component tests (React Testing Library)
  - [ ] Integration tests
  - [ ] E2E tests (Playwright or Cypress)
- [ ] **Manual Testing Checklist**
  - [ ] Complete user journey testing
  - [ ] Cross-browser testing (Chrome, Firefox, Safari)
  - [ ] Mobile responsiveness testing
  - [ ] Edge case testing
  - [ ] Regression testing

#### Documentation
- [ ] **Player Documentation**
  - [ ] Game rules and mechanics
  - [ ] Strategy guide
  - [ ] FAQ
  - [ ] Troubleshooting guide
- [ ] **Developer Documentation**
  - [ ] API documentation (Swagger + written docs)
  - [ ] Architecture overview
  - [ ] Database schema documentation
  - [ ] Deployment guide
  - [ ] Contributing guide
- [ ] **Code Documentation**
  - [ ] XML comments on public APIs
  - [ ] Component documentation
  - [ ] README files for each major module

#### Deployment
- [ ] **Railway Setup**
  - [ ] PostgreSQL database provisioning
  - [ ] Redis instance setup
  - [ ] Environment variables configuration
  - [ ] Domain configuration
- [ ] **Backend Deployment**
  - [ ] Docker containerization
  - [ ] CI/CD pipeline (GitHub Actions)
  - [ ] Database migration strategy
  - [ ] Secrets management
  - [ ] Health checks
- [ ] **Frontend Deployment**
  - [ ] Next.js production build
  - [ ] Static asset hosting
  - [ ] Environment configuration
  - [ ] CDN setup (optional)
- [ ] **Monitoring & Logging**
  - [ ] Application monitoring
  - [ ] Error tracking
  - [ ] Performance monitoring
  - [ ] Log aggregation

#### Pre-Launch Checklist
- [ ] All critical bugs fixed
- [ ] Security audit completed
- [ ] Performance benchmarks met
- [ ] Backup and restore procedures tested
- [ ] Rollback plan documented
- [ ] User feedback mechanism in place
- [ ] Terms of service and privacy policy
- [ ] Beta testing period completed

#### Success Criteria
- 70%+ test coverage
- All E2E tests passing
- Documentation complete and accurate
- Successful deployment to Railway
- Zero critical bugs
- Performance targets met
- Production monitoring active
- Game is publicly accessible

---

## Post-Launch Phases (Future Enhancements)

### Phase 13: Multiplayer PvP & Diplomacy (Optional)
**Timeline**: 2-3 weeks

- [ ] Player vs player combat
- [ ] Diplomatic relations system
- [ ] Alliances and treaties
- [ ] Territory control mechanics
- [ ] Fog of war
- [ ] Leaderboards
- [ ] Chat system

### Phase 14: Advanced Features (Optional)
**Timeline**: 3-4 weeks

- [ ] Research and technology trees
- [ ] Economic systems (trading, markets)
- [ ] Guild/alliance systems
- [ ] Advanced agent behaviors
- [ ] LLM-powered AI opponents
- [ ] 3D galaxy visualization
- [ ] Mobile app (React Native)
- [ ] Replay system
- [ ] Modding support

---

## Technical Architecture

### Data Model
```
User (ASP.NET Identity)
  └─ PlayerGame (Junction)
       └─ Game ── Galaxy ── StarSystem ── Planet ── GridSquare
                                         │          └─ Building
                                         ├─ Wormhole (connects systems)
                                         └─ SpaceStation ── Shipyard ── Spaceship
  └─ Agent (Automated behaviors)
  └─ Builder (Construction units)
```

### Key Design Principles

1. **Real-time Continuity**: Game state updates continuously, not only on player action
2. **Extensibility**: Plugin-based architecture for behaviors, resources, building types
3. **Scalability**: Horizontal scaling ready with Redis pub/sub
4. **Performance**: Efficient batch processing and caching strategies
5. **Type Safety**: Full TypeScript and C# type safety throughout
6. **User Experience**: Intuitive UI with comprehensive onboarding

### Infrastructure Patterns

- **Background Jobs**: Worker service for simulation ticks
- **Event-Driven**: SignalR for real-time updates
- **Caching Strategy**: Redis for hot data, PostgreSQL for persistence
- **Migration Strategy**: EF Core migrations with version control
- **API Design**: RESTful with consistent DTO patterns
- **Testing Strategy**: Unit, integration, and E2E testing

---

## Timeline to Launch

### Completed (Phases 1-7): ~10 weeks ✅
- Core infrastructure
- Galaxy generation
- Planet grids and resources
- Building system
- Real-time simulation
- Agent behaviors
- Spaceships and shipyards

### Remaining for Launch (Phases 8-12): ~7-8 weeks ⏳
- **Phase 8**: Combat System (1-2 weeks)
- **Phase 9**: UI/UX Polish (2 weeks)
- **Phase 10**: Game Balance (1 week)
- **Phase 11**: Performance Optimization (1 week)
- **Phase 12**: Testing & Deployment (2 weeks)

### Total Project Timeline
- **Completed**: 70% (10 weeks)
- **Remaining**: 30% (7-8 weeks)
- **Total**: 17-18 weeks to launch
- **Expected Launch**: ~2 months from now

### Post-Launch (Optional): 5-7 weeks
- **Phase 13**: Multiplayer PvP (2-3 weeks)
- **Phase 14**: Advanced Features (3-4 weeks)

---

## Risk Mitigation

### Technical Risks

1. **Performance at Scale**: Large galaxy generation and simulation
   - Mitigation: Phase 11 dedicated to optimization, load testing, monitoring

2. **Data Consistency**: Real-time updates vs. offline calculations
   - Mitigation: Existing simulation engine handles this, additional testing in Phase 12

3. **Combat Balance**: Making combat fun and balanced
   - Mitigation: Iterative design in Phase 8, playtesting in Phase 10

### Project Risks

1. **Scope Creep**: Adding features beyond MVP
   - Mitigation: Strict phase boundaries, phases 13-14 are post-launch only

2. **Testing Gaps**: Insufficient testing before launch
   - Mitigation: Dedicated phase 12 for comprehensive testing

3. **User Adoption**: Players not understanding the game
   - Mitigation: Phase 9 focused on onboarding and UX

---

## Success Criteria for Launch

### Minimum Viable Product (MVP) Requirements

#### Core Gameplay
- ✅ Players can authenticate and create accounts
- ✅ Players can create/join game instances
- ✅ Galaxies generate with systems and planets
- ✅ Resources can be extracted and managed
- ✅ Buildings construct over time
- ✅ Ships can be built and moved
- ✅ Agents can automate tasks
- ⏳ Combat system is functional
- ⏳ Game is balanced and fun

#### User Experience
- ⏳ Tutorial teaches core mechanics
- ⏳ UI is polished and consistent
- ⏳ All features have tooltips/help
- ⏳ Loading and error states are clear
- ⏳ Documentation is complete

#### Technical
- ⏳ Performance meets targets (< 200ms API, < 3s load)
- ⏳ 70%+ test coverage
- ⏳ Zero critical bugs
- ⏳ Deployed to production (Railway)
- ⏳ Monitoring and logging active

#### Quality
- ⏳ Cross-browser compatible
- ⏳ Mobile responsive
- ⏳ Secure (security audit passed)
- ⏳ Accessible (WCAG 2.1 Level A minimum)

### Launch Readiness Checklist

- [ ] All Phases 1-12 complete
- [ ] Beta testing completed with 10+ users
- [ ] All critical and high-priority bugs fixed
- [ ] Performance benchmarks met
- [ ] Documentation published
- [ ] Terms of service and privacy policy ready
- [ ] Marketing materials prepared
- [ ] Support channels established
- [ ] Production deployment tested
- [ ] Rollback plan verified

---

## Development Workflow

### Branch Strategy
- `main`: Production-ready code
- `develop`: Integration branch
- `feature/*`: Feature branches for each phase
- `hotfix/*`: Critical production fixes

### Testing Strategy
- Unit tests for core logic
- Integration tests for API endpoints
- E2E tests for user flows (Phase 12)
- Performance testing for large galaxies (Phase 11)
- Manual testing throughout

### Deployment Strategy
- **Development**: Local with Docker Compose
- **Staging**: Railway staging environment (Phase 12)
- **Production**: Railway production with monitoring (Phase 12)

---

## Metrics for Success

### Player Engagement (Post-Launch)
- Daily Active Users (DAU)
- Session length (target: 30+ minutes)
- Retention rate (Day 1, Day 7, Day 30)
- Tutorial completion rate (target: 80%+)

### Technical Metrics
- API response time (target: < 200ms p95)
- Frontend load time (target: < 3s)
- Uptime (target: 99.5%+)
- Error rate (target: < 0.1%)

### Quality Metrics
- Bug count by severity
- Test coverage percentage (target: 70%+)
- Code review completion rate (target: 100%)

---

## Future Enhancements (Post-MVP)

### High Priority (Post-Launch)
- Player vs player combat (Phase 13)
- Diplomatic relations (Phase 13)
- Alliances and guilds (Phase 13)
- Leaderboards and rankings (Phase 13)

### Medium Priority
- Research and technology trees
- Economic systems and trading
- Advanced combat tactics
- Mobile client (React Native)
- 3D galaxy visualization

### Low Priority
- Spectator mode
- Replay system
- AI opponents
- Modding support
- VR support

---

## Conclusion

This replanned project roadmap ensures **System** will launch as a polished, balanced, and engaging multiplayer 4X strategy game. With 70% of core features already complete, the remaining 7-8 weeks focus on:

1. **Combat** - Adding the final core gameplay mechanic
2. **Polish** - Making the game beautiful and intuitive
3. **Balance** - Ensuring fun and fair gameplay
4. **Performance** - Delivering smooth, reliable experience
5. **Testing & Launch** - Comprehensive quality assurance and deployment

The game will be feature-complete, well-tested, and ready for public launch after Phase 12.

---

**Document Version**: 2.0  
**Last Updated**: 2025-10-31 (Recheckpoint after Phase 7)  
**Status**: 70% Complete - On Track for Launch  
**Next Phase**: Phase 8 - Combat System & NPCs  
**Expected Launch**: ~2 months from recheckpoint
