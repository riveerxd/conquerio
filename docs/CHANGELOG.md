# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2026-03-06]
### Fixed
- Merge pull request #47 from riveerxd/fix/room-endpoints (Lukáš Hrehor)
- re-add room endpoints lost in leaderboard merge (lukas.hrehor)
### Changed
- Merge pull request #44 from riveerxd/feature/PlayerState (Lukáš Hrehor)
- remove getter/setter tests that just test c# auto-properties (lukas.hrehor)
### Added
- Merge pull request #40 from riveerxd/feature/GameRoomManager (Lukáš Hrehor)
- Merge pull request #39 from riveerxd/feature/GameRoomUnitTests (Lukáš Hrehor)
- Merge pull request #41 from riveerxd/feature/CollisionDetector (Lukáš Hrehor)

## [2026-03-05]
### Added
- Unit tests for playerstate (kills, startedat, stats, boost, trail, defaults) (InkaEnFu)
- Unit tests for territory and bounds (InkaEnFu)
- Unit tests for game room management (find, cleanup, mark, cancel, get, create) (InkaEnFu)
- Unit tests for grid and player operations (getdelta, isopposite, getflatgrid, isfull, removeplayer, addplayer) (InkaEnFu)
- Implement CollisionDetector.cs logic (Stevek)
- Add game room browser (lukas.hrehor)
- Add test job to ci pipeline before deploy (lukas.hrehor)
### Fixed
- Fix merge conflict (Stevek)
- Fix review issues (Štěpán Végh)
### Changed
- Merge pull request #36 from riveerxd/feature/collisions (Lukáš Hrehor)
- Merge branch 'main' into feature/collisions (Štěpán Végh)
- Merge pull request #26 from riveerxd/feature/leaderboard (Štěpán Végh)
- Merge branch 'main' into feature/leaderboard (Stevek)
- Merge pull request #33 from riveerxd/feature/room-browser (Lukáš Hrehor)
- Merge pull request #30 from riveerxd/enhancement/ci-test-step (Lukáš Hrehor)
- Merge pull request #28 from riveerxd/feature/TerritoryResolver (Lukáš Hrehor)
- Move unit tests into backend and fix project setup (lukas.hrehor)

## [2026-03-04]
### Added
- Unit tests for territory resolver edge cases (trail along grid border, l-shaped and overlapping) (InkaEnFu)
- Unit tests project for territory resolver (InkaEnFu)
- Add spacebar boost trigger to frontend (lukas.hrehor)
- Add to websocket endpoints (Stevek)
- Implement player deaths (Stevek)
- Implement endpoint functionality (Stevek)
- Implemented boost cooldown and runout (Jan Koubek)
- Added ability handling in game room tick (Jan Koubek)
- Created player ability enum (Jan Koubek)
- Added ability property in client message (Jan Koubek)
### Fixed
- Fix all PR review issues: async void, race condition, grid scan, array alloc, rank projection, Elo TODO (copilot-swe-agent[bot])
- Fixed own territory checks (Jan Koubek)
- Fixed trail spaces getting added multiple times (Jan Koubek)
- Fixed the boost gaps bug by calculating the spaces the player traveled through (Jan Koubek)
### Changed
- Merge pull request #25 from riveerxd/Fix/boost-gaps (Lukáš Hrehor)
- Merge pull request #27 from riveerxd/copilot/fix-pr-review-issues (Štěpán Végh)
- Initial plan (copilot-swe-agent[bot])
- Modified player input to include ability (Jan Koubek)
- Implemented ability handling in web socket endpoint (Jan Koubek)
- Applied speed multiplier in tick (Jan Koubek)
- Modified player state speed multiplier to integer (Jan Koubek)
- Merge pull request #22 from riveerxd/feature/territory-claiming (riveerxd)
- Implement territory claiming with flood fill (lukas.hrehor)

## [2026-03-03]
### Added
- Add health check endpoint (lukas.hrehor)
- Switch from EnsureCreated to EF Core migrations (lukas.hrehor)
- Add minimap to game view (lukas.hrehor)
- Add contributing guide (lukas.hrehor)
- Add login screen and auth api (lukas.hrehor)
- Add frontend game engine and canvas rendering (lukas.hrehor)
- Add deploy pipeline (lukas.hrehor)
- Add websocket protocol and message types (lukas.hrehor)
- Add game loop and server tick system (lukas.hrehor)
- Add docker-compose and env setup (lukas.hrehor)
- Add frontend dockerfile (lukas.hrehor)
- Init frontend with vite react ts (lukas.hrehor)
- Add backend dockerfile (lukas.hrehor)
### Fixed
- Fix ts build errors and node_modules permissions in deploy pipeline (lukas.hrehor)
- Fix login to use username instead of email (lukas.hrehor)
### Changed
- Merge pull request #20 from riveerxd/feature/health-endpoint (riveerxd)
- Merge pull request #18 from riveerxd/feature/ef-core-migrations (riveerxd)
- Merge pull request #16 from riveerxd/fix/prod-build-errors (riveerxd)
- Merge pull request #15 from riveerxd/feature/prod-builds (riveerxd)
- Switch to production builds, serve frontend from nginx (lukas.hrehor)
- Merge pull request #13 from riveerxd/feature/minimap (riveerxd)
- Update readme with current stack and deploy info (lukas.hrehor)
- Merge pull request #1 from riveerxd/feature/game-loop (riveerxd)
- Switch deploy pipeline to production branch (lukas.hrehor)
- Wire up app routing between login and game (lukas.hrehor)
- Allow all hosts in vite dev server (lukas.hrehor)
- Rewrite ws endpoint with jwt auth (lukas.hrehor)
- Switch from sqlite to mysql (lukas.hrehor)
- Move docs and non-code files (lukas.hrehor)
- Move backend to its own directory (lukas.hrehor)

## [2026-03-02]
- Create CHANGELOG. md (Štěpán Végh)

## [2026-03-01]
### Added
- Add example requests (Stevek)
- Add remaining db models (Stevek)
- Add endpoint placeholders (Stevek)
- Add models (Stevek)
- Add auth endpoints (Stevek)
- Add IDE settings (Stevek)
- Create the game API template (Stevek)
### Fixed
- Make the JWT key longer to work (Stevek)
- Various fixes (Stevek)
### Changed
- Remove IDE config (Stevek)
- Add db to gitignore (Stevek)

## [2026-02-24]
- Create work_division.txt (Štěpán Végh)
- Added readme, techstack (lukas.hrehor)
- Added design doc (lukas.hrehor)

## [2026-02-19]
- Initial commit (stevek)

## [2026-01-03]
- Created API endpoints (Štěpán Végh)
- Implemented login and register logic (Štěpán Végh)
- Created DB schema and entities (Štěpán Végh)
