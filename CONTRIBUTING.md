# How to work on this project

## Branches

- `main` - latest code, everything gets merged here
- `production` - deployed to the server, only Hrehor merges into this
- `feature/*` - new stuff
- `fix/*` - bug fixes

**Never push directly to `main` or `production`.**

## Workflow

1. Pick an issue assigned to you from [Issues](https://github.com/riveerxd/conquerio/issues)
2. Create a branch from `main`:
   ```
   git checkout main
   git pull
   git checkout -b feature/your-feature-name
   ```
3. Do your work, commit often
4. Push your branch:
   ```
   git push -u origin feature/your-feature-name
   ```
5. Create a Pull Request to `main` on GitHub
6. Wait for review/approval
7. Once merged, delete your branch

## Commit messages

Keep them short and lowercase. Examples:
- `add trail collision detection`
- `fix player respawn position`
- `update leaderboard endpoint`

## Running locally

```
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

- Frontend: http://localhost:5173
- Backend API: http://localhost:8080
- MySQL: localhost:3306

## Project structure

```
backend/                  # ASP.NET Core 9
  Endpoints/              # API routes
  Game/                   # Game logic (rooms, tick loop, collisions)
    Messages/             # WebSocket message types
  Models/                 # Database entities
  Data/                   # EF Core context

frontend/                 # React + Vite + TypeScript
  src/
    game/                 # Game engine (rendering, networking, input)
    ui/                   # UI screens (login, death, etc)
    api/                  # API client functions
```

## Who does what

| Person | GitHub | Area | What to touch |
|--------|--------|------|---------------|
| Jenda | @KoubekJ1 | Backend | `backend/Game/GameRoom.cs`, `backend/Game/TerritoryResolver.cs` |
| Stepan | @Stevekk11 | Backend | `backend/Game/CollisionDetector.cs`, `backend/Endpoints/GameEndpoints.cs` |
| Sebastian | @Se8o | Frontend | `frontend/src/game/`, `frontend/src/ui/` |
| Prokop | @InkaEnFu | Testing | Unit tests for game logic, integration tests |
| Hrehor | @riveerxd | Infra/Lead | Architecture, CI/CD, code review, merging to production |

## Questions?

Ask in the group chat or open an issue.
