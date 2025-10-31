# System Game - Quick Start Guide

## Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **PostgreSQL** - [Download](https://www.postgresql.org/download/)
- **Redis** - [Download](https://redis.io/download)

### Installing Dependencies (macOS)

```bash
# PostgreSQL
brew install postgresql@16
brew services start postgresql@16

# Redis
brew install redis
brew services start redis
```

## Getting Started

### 1. Clone and Setup Backend

```bash
cd system-be

# Restore packages
dotnet restore

# Build the project
dotnet build

# Create database
createdb systemgame

# Run migrations
cd SystemGame.Api
dotnet ef database update

# Run the API
dotnet run
```

The API will be available at `http://localhost:5002`
Swagger UI: `http://localhost:5002/swagger`

### 2. Setup Frontend

In a new terminal:

```bash
cd system-fe

# Install dependencies
npm install

# Create environment file
echo "NEXT_PUBLIC_API_URL=http://localhost:5002" > .env.local

# Run development server
npm run dev
```

The frontend will be available at `http://localhost:5001`

### 3. Test Authentication

1. Navigate to `http://localhost:5001`
2. Click "Register" to create an account
3. Fill in the registration form
4. You'll be redirected to the dashboard
5. Test logout and login functionality

## Development Workflow

### Backend Changes

After making changes to entities or models:
```bash
# Create a new migration
dotnet ef migrations add MigrationName --project SystemGame.Api

# Apply migrations
dotnet ef database update --project SystemGame.Api
```

### Frontend Changes

The Next.js dev server supports hot reload. Just save your files and changes will appear immediately.

## Troubleshooting

### Database Connection Issues

If you get PostgreSQL connection errors:
```bash
# Check if PostgreSQL is running
pg_isready

# Check connection string in appsettings.json
# Default: Host=localhost;Database=systemgame;Username=postgres;Password=postgres
```

### Redis Connection Issues

If Redis fails to connect:
```bash
# Check if Redis is running
redis-cli ping
# Should return: PONG

# Redis is configured with retry logic, so the app will still start
# but some features won't work
```

### Frontend Can't Connect to Backend

Make sure:
1. Backend is running on port 5000
2. `.env.local` has the correct API URL
3. CORS is configured (already set in backend)

### Migration Errors

If you get migration errors:
```bash
# Drop and recreate the database
dropdb systemgame
createdb systemgame
dotnet ef database update --project SystemGame.Api
```

## Project Commands

### Backend
```bash
# Build
dotnet build

# Run
dotnet run --project SystemGame.Api

# Run tests (when available)
dotnet test

# Clean
dotnet clean
```

### Frontend
```bash
# Development
npm run dev

# Build
npm run build

# Production
npm start

# Lint
npm run lint
```

## Architecture Overview

- **Backend**: C# .NET 9 Web API with Entity Framework Core
- **Frontend**: Next.js 16 with React 19, TypeScript
- **Database**: PostgreSQL 16+
- **Cache**: Redis 7+
- **Real-time**: SignalR (configured, not yet used)
- **Auth**: JWT tokens with Redis-backed refresh tokens

## Next Steps

After Phase 1 is working, proceed to Phase 2:
- Galaxy generation
- System and planet data models
- Resource distribution
- Game instance management

See `Overview.md` for full project requirements and `Phase1_Complete.md` for what's been implemented.

