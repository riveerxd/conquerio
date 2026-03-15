# Changelog

---

## [2026-03-13]

### lukas.hrehor
- show killer username instead of UUID on death screen
- fix self-trail death + add encirclement kills
- fix self-trail collision to claim territory when loop is complete
- fix grid sync for new players joining ongoing game
- fix death by enemy trail: send death message + clear territory
- fix stale unit tests
- remove stale GetFlatGridTest

---

## [2026-03-12]

### Se8o
- test: assert username is included in WS state message (#52)
- refactor: enhance Leaderboard with throttled updates, improved Elo matching, and proper network client unsubscription
- fix: resolve merge conflict with main - add DeathScreen alongside Leaderboard

### lukas.hrehor
- added codeowners

---

## [2026-03-11]

### Stevek
- RLE decoder in frontend
- fix player labels in frontend to show
- fix player labels in backend

---

## [2026-03-10]

### Se8o
- feat: wire up death screen and respawn flow
- feat: improve trail and territory rendering
- feat: add in-game leaderboard overlay

### Stevek
- add serilog server-side logging
- add reconnection handling
- implement grid compression
- create Elo calculator with standard formula and K-factor of 32
- added Elo integration
- logging for different actions

### InkaEnFu
- add playwright dev dependency
- add auth register and login tests
- add death and respawn tests
- add join game and movement tests
- add leaderboard and stats endpoint tests
- add room browser api tests
- fix boost ability test referencing missing property

### lukas.hrehor
- fix test isolation and remove flaky delays
- remove junk comment

---

## [2026-03-09]

### Se8o
- feat: add password validation and hint for user registration

### Štěpán Végh
- create changelog from git commits

---

## [2026-03-08]

### Stevek
- create changelog from git commits

---

## [2026-03-06]

### InkaEnFu
- add websocket integration test dependencies
- add websocket test factory and base class
- add valid jwt connection test
- add invalid token rejection tests
- add full room rejection test
- add disconnect player removal test
- add direction input movement test
- add boost ability speed test
- add multi client state update test

### Stevek
- add head-on collision handling
- review code

### Se8o
- fix: show real error messages on login/register
- build: add package lock file

### lukas.hrehor
- re-add room endpoints lost in leaderboard merge
- remove getter/setter tests that just test c# auto-properties

---

## [2026-03-05]

### InkaEnFu
- add addplayer unit tests
- add removeplayer unit tests
- add cleanupemptyrooms unit tests
- add createroom unit tests
- add findroomforplayer unit tests
- add getorcreateroom unit tests
- add getroom unit tests
- add getdelta unit tests
- add getflatgrid unit tests
- add isfull unit tests
- add isopposite unit tests
- add isoutofbounds unit tests
- add hitstrail unit tests
- add hitsselftrail unit tests
- add markempty cancelempty unit tests
- add player on own territory unit tests
- add playerstate boost unit tests
- add playerstate defaults unit tests
- add playerstate kills unit tests
- add playerstate startedat unit test
- add playerstate stats unit tests
- add playerstate trail unit tests

### Stevek
- implement CollisionDetector.cs logic
- fix merge conflict

### Štěpán Végh
- fix review issues

### lukas.hrehor
- add game room browser
- add test job to CI pipeline before deploy
- move unit tests into backend and fix project setup

---

## [2026-03-04]

### Jan Koubek
- created player ability enum
- added ability property in client message
- added ability handling in game room tick
- implemented ability handling in websocket endpoint
- applied speed multiplier in tick
- modified player state speed multiplier to integer
- modified player input to include ability
- implemented boost cooldown and runout
- fixed own territory checks
- fixed trail spaces getting added multiple times
- fixed the boost gaps bug by calculating the spaces the player traveled through

### Stevek
- add to websocket endpoints
- implement endpoint functionality
- implement player deaths

### InkaEnFu
- add unit tests project for territory resolver
- add simple square loop territory test
- add trail along grid border edge case test
- add l-shaped and overlapping territory tests

### lukas.hrehor
- implement territory claiming with flood fill
- add spacebar boost trigger to frontend

### copilot-swe-agent[bot]
- fix all PR review issues: async void, race condition, grid scan, array alloc, rank projection, Elo TODO

---

## [2026-03-03]

### lukas.hrehor
- add backend dockerfile
- add frontend dockerfile
- add docker-compose and env setup
- add deploy pipeline
- add contributing guide
- add game loop and server tick system
- add websocket protocol and message types
- add login screen and auth api
- add health check endpoint
- add frontend game engine and canvas rendering
- add minimap to game view
- init frontend with vite react ts
- switch from sqlite to mysql
- switch from EnsureCreated to EF Core migrations
- switch to production builds, serve frontend from nginx
- rewrite ws endpoint with jwt auth
- switch deploy pipeline to production branch
- move backend to its own directory
- move docs and non-code files
- wire up app routing between login and game
- allow all hosts in vite dev server
- fix login to use username instead of email
- fix ts build errors and node_modules permissions in deploy pipeline
- update readme with current stack and deploy info

---

## [2026-03-02]

### Štěpán Végh
- create CHANGELOG.md

---

## [2026-03-01]

### Stevek
- initial commit: create the game API template
- add auth endpoints
- add endpoint placeholders
- add example requests
- add models
- add remaining db models
- add IDE settings
- make the JWT key longer to work
- remove IDE config
- add db to gitignore
- various fixes

---

## [2026-02-24]

### lukas.hrehor
- added design doc
- added readme, techstack

### Štěpán Végh
- create work_division.txt

---

## [2026-02-19]

### stevek
- initial commit
