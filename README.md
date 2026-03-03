# Conquer.io

Multiplayer territory control game inspired by Paper.io. Claim land, defend your turf, kill other players.

**Live at:** https://conquerio.riveer.cz

## Tech stack

- **Frontend** - React + Vite + TypeScript, canvas rendering
- **Backend** - ASP.NET Core 9, raw WebSockets, JWT auth
- **Database** - MySQL 8.0
- **Infra** - Docker Compose, GitHub Actions CI/CD

## Running locally

```
docker compose up --build
```

- Frontend: http://localhost:5173
- Backend: http://localhost:8080
- MySQL: localhost:3306

## Project structure

```
backend/                  # ASP.NET Core API
  Endpoints/              # REST + WebSocket routes
  Game/                   # Game loop, rooms, tick system
    Messages/             # WebSocket protocol types
  Models/                 # DB entities
  Data/                   # EF Core context

frontend/                 # React + Vite
  src/
    game/                 # Canvas renderer, networking, input
    ui/                   # Login, death screen, etc
    api/                  # API client
```

## Deployment

```
push to production branch → GitHub Actions → SSH into VPS → docker compose up --build
```

- Only @riveerxd can merge into `production` (branch protection)
- Workflow: `.github/workflows/deploy.yml`
- VPS runs nginx with SSL (certbot) reverse proxying to the containers
- Secrets (SSH key, VPS host) stored in GitHub repo secrets

## Architecture

Server-authoritative multiplayer. Server runs a 20Hz tick loop, clients send direction input, server broadcasts game state. No client-side prediction, just interpolation between ticks.

## Team

See [CONTRIBUTING.md](CONTRIBUTING.md) for workflow and work distribution.
