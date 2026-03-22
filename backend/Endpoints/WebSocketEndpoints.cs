using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using conquerio.Data;
using conquerio.Game;
using conquerio.Game.Abilities;
using conquerio.Game.Messages;
using conquerio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace conquerio.Endpoints;

public static class WebSocketEndpoints
{
    public static void MapWebSocketEndpoints(this WebApplication app)
    {
        app.Map("/ws/game", async (HttpContext context, GameRoomManager roomManager,
            UserManager<AppUser> userManager, IConfiguration config,
            IServiceScopeFactory scopeFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("WebSocketEndpoints");

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
        

            // auth from query param
            var token = context.Request.Query["token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var principal = ValidateToken(token, config);
            if (principal == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // figure out which room to join
            var roomId = context.Request.Query["roomId"].FirstOrDefault();
            GameRoom room;

            if (!string.IsNullOrEmpty(roomId))
            {
                var target = roomManager.GetRoom(roomId);
                if (target == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                if (target.IsFull)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    return;
                }
                room = target;
            }
            else
            {
                room = roomManager.GetOrCreateRoom();
            }

            // cancel cleanup timer if someone joins an empty room
            roomManager.CancelEmpty(room.RoomId);

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            var player = room.AddPlayer(userId, user.UserName ?? "unknown", ws);
            var deathEvents = Channel.CreateUnbounded<PlayerDeathEvent>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            var deathPersistenceTask = Task.Run(async () =>
            {
                await foreach (var evt in deathEvents.Reader.ReadAllAsync())
                {
                    await PersistDeathStatsAsync(scopeFactory, logger, evt);
                }
            });

            // named local function so we can unsubscribe it in the finally block
            void OnPlayerDied(PlayerDeathEvent evt)
            {
                if (evt.VictimId != userId)
                    return;

                if (!deathEvents.Writer.TryWrite(evt))
                {
                    logger.LogWarning("Dropped death event for user {UserId}; death queue writer was closed.", userId);
                }
            }

            void OnPlayerWon(PlayerWonEvent evt)
            {
                if (evt.WinnerId != userId) return;
                _ = PersistWinStatsAsync(scopeFactory, logger, evt);
            }

            room.PlayerDied += OnPlayerDied;
            room.PlayerWon += OnPlayerWon;

            // send joined message with full grid
            await MessageSerializer.SendAsync(ws, new JoinedMessage
            {
                PlayerId = userId,
                ColorId = player.ColorId,
                GridWidth = room.GridWidth,
                GridHeight = room.GridHeight,
                TickRate = room.TickRate,
                RleGrid = room.GetRleGrid()
            });

            // read loop
            var buffer = new byte[1024];
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                        break;
                    }

                    var msg = MessageSerializer.Deserialize(buffer, result.Count);
                    if (msg == null) continue;

                    switch (msg.Type)
                    {
                        case "input":
                            if (msg.Dir != null && Enum.TryParse<Direction>(msg.Dir, true, out var dir))
                            {
                                room.InputQueue.Enqueue(new PlayerInput
                                {
                                    PlayerId = userId,
                                    Direction = dir
                                });
                            }
                            break;

                        case "ability":
                            if (msg.Ability != null)
                            {
                                room.InputQueue.Enqueue(new PlayerInput
                                {
                                    PlayerId = userId,
                                    Ability = msg.Ability
                                });
                            }
                            break;

                        case "ping":
                            await MessageSerializer.SendAsync(ws, new PongMessage { T = msg.T ?? 0 });
                            break;
                    }
                }
            }
            catch (WebSocketException) { } // client disconnected
            finally
            {
                room.MarkDisconnected(userId);
                room.PlayerDied -= OnPlayerDied;
                room.PlayerWon -= OnPlayerWon;

                deathEvents.Writer.TryComplete();
                try
                {
                    await deathPersistenceTask;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Death persistence worker failed for user {UserId}", userId);
                }

                // Wait a bit to see if they actually timed out before checking if room should be marked empty
                await Task.Delay(11000);

                if (room.Players.TryGetValue(userId, out var p) && !p.IsAlive)
                {
                    room.RemovePlayer(userId);
                }

                if (room.Players.Values.All(ps => ps.IsDisconnected || !ps.IsAlive))
                    roomManager.MarkEmpty(room.RoomId);
            }
        })
        .WithTags("Game")
        .WithSummary("WebSocket game connection")
        .WithDescription("Connect to the game server via WebSocket. Requires a valid JWT token in the 'token' query parameter and an optional 'roomId'.");
    }

    private static async Task PersistWinStatsAsync(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        PlayerWonEvent evt)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entry = await db.Leaderboard.FirstOrDefaultAsync(lb => lb.UserId == evt.WinnerId);
        int newElo = (entry?.Elo ?? 1000) + 100;

        try
        {
            db.GameRuns.Add(new GameRun
            {
                UserId = evt.WinnerId,
                Kills = evt.Kills,
                MaxTerritoryPct = evt.MaxTerritoryPct,
                DeathCause = "won",
                StartedAt = evt.StartedAtUtc,
                UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save game run for winner {WinnerId}", evt.WinnerId);
        }

        try
        {
            await UpsertPlayerStatsAsync(db, evt.WinnerId, evt.Kills, evt.MaxTerritoryPct, newElo, isDeath: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert player stats for winner {WinnerId}", evt.WinnerId);
        }

        try
        {
            await UpsertLeaderboardAsync(db, evt.WinnerId, evt.MaxTerritoryPct, newElo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert leaderboard for winner {WinnerId}", evt.WinnerId);
        }
    }

    private static async Task PersistDeathStatsAsync(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        PlayerDeathEvent evt)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Fetch current Elo from Leaderboard (default 1000)
        var victimEntry = await db.Leaderboard
            .FirstOrDefaultAsync(lb => lb.UserId == evt.VictimId);
        int victimOldElo = victimEntry?.Elo ?? 1000;

        int? killerOldElo = null;
        if (!string.IsNullOrEmpty(evt.KillerId))
        {
            var killerEntry = await db.Leaderboard
                .FirstOrDefaultAsync(lb => lb.UserId == evt.KillerId);
            killerOldElo = killerEntry?.Elo ?? 1000;
        }

        // Calculate new Elo
        var (victimNewElo, killerNewElo) = EloCalculator.Calculate(
            victimOldElo,
            killerOldElo,
            evt.MaxTerritoryPct);

        try
        {
            db.GameRuns.Add(new GameRun
            {
                UserId = evt.VictimId,
                Kills = evt.Kills,
                MaxTerritoryPct = evt.MaxTerritoryPct,
                DeathCause = evt.DeathCause,
                StartedAt = evt.StartedAtUtc,
                UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save game run for {VictimId}", evt.VictimId);
        }

        try
        {
            await UpsertPlayerStatsAsync(db, evt.VictimId, evt.Kills, evt.MaxTerritoryPct, victimNewElo);
            if (!string.IsNullOrEmpty(evt.KillerId) && killerNewElo.HasValue)
            {
                // For killer, we only update Elo (kills/deaths are handled when they die, or maybe we want to increment kills here).
                // Usually kills are incremented in real-time or when the game session ends.
                // Let's increment kills for the killer now!
                await UpsertPlayerStatsAsync(db, evt.KillerId, killsInc: 1, maxPct: 0, newElo: killerNewElo.Value, isDeath: false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert player stats for {VictimId}", evt.VictimId);
        }

        try
        {
            await UpsertLeaderboardAsync(db, evt.VictimId, evt.MaxTerritoryPct, victimNewElo);
            if (!string.IsNullOrEmpty(evt.KillerId) && killerNewElo.HasValue)
            {
                await UpsertLeaderboardAsync(db, evt.KillerId, maxPct: 0, newElo: killerNewElo.Value);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert leaderboard for {VictimId}", evt.VictimId);
        }
    }

    private static async Task UpsertPlayerStatsAsync(
        AppDbContext db,
        string userId,
        int killsInc,
        float maxPct,
        int newElo,
        bool isDeath = true)
    {
        int statsUpdated = await db.PlayerStats
            .Where(ps => ps.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(ps => ps.TotalGames, ps => isDeath ? ps.TotalGames + 1 : ps.TotalGames)
                .SetProperty(ps => ps.TotalKills, ps => ps.TotalKills + killsInc)
                .SetProperty(ps => ps.TotalDeaths, ps => isDeath ? ps.TotalDeaths + 1 : ps.TotalDeaths)
                .SetProperty(ps => ps.Elo, ps => newElo)
                .SetProperty(ps => ps.BestTerritoryPct,
                    ps => maxPct > ps.BestTerritoryPct ? maxPct : ps.BestTerritoryPct));

        if (statsUpdated != 0)
            return;

        db.PlayerStats.Add(new PlayerStats
        {
            UserId = userId,
            TotalGames = isDeath ? 1 : 0,
            TotalKills = killsInc,
            TotalDeaths = isDeath ? 1 : 0,
            Elo = newElo,
            BestTerritoryPct = maxPct
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            await db.PlayerStats
                .Where(ps => ps.UserId == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(ps => ps.TotalGames, ps => isDeath ? ps.TotalGames + 1 : ps.TotalGames)
                    .SetProperty(ps => ps.TotalKills, ps => ps.TotalKills + killsInc)
                    .SetProperty(ps => ps.TotalDeaths, ps => isDeath ? ps.TotalDeaths + 1 : ps.TotalDeaths)
                    .SetProperty(ps => ps.Elo, ps => newElo)
                    .SetProperty(ps => ps.BestTerritoryPct,
                        ps => maxPct > ps.BestTerritoryPct ? maxPct : ps.BestTerritoryPct));
        }
    }

    private static async Task UpsertLeaderboardAsync(AppDbContext db, string userId, float maxPct, int newElo)
    {
        int lbUpdated = await db.Leaderboard
            .Where(lb => lb.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(lb => lb.Elo, lb => newElo)
                .SetProperty(lb => lb.BestPct,
                    lb => maxPct > lb.BestPct ? maxPct : lb.BestPct));

        if (lbUpdated != 0)
            return;

        db.Leaderboard.Add(new Leaderboard
        {
            UserId = userId,
            Elo = newElo,
            BestPct = maxPct
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            await db.Leaderboard
                .Where(lb => lb.UserId == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(lb => lb.Elo, lb => newElo)
                    .SetProperty(lb => lb.BestPct,
                        lb => maxPct > lb.BestPct ? maxPct : lb.BestPct));
        }
    }

    private static ClaimsPrincipal? ValidateToken(string token, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        var handler = new JwtSecurityTokenHandler();
        try
        {
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}
