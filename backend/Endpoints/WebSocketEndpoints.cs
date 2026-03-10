using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using conquerio.Data;
using conquerio.Game;
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

            room.PlayerDied += OnPlayerDied;

            // send joined message with compressed grid
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
                            if (msg.Ability != null && Enum.TryParse<PlayerAbility>(msg.Ability, true, out var ability))
                            {
                                room.InputQueue.Enqueue(new PlayerInput
                                {
                                    PlayerId = userId,
                                    Ability = ability
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
                room.TryKillPlayer(userId, killerId: null, cause: "disconnect");
                room.PlayerDied -= OnPlayerDied;

                deathEvents.Writer.TryComplete();
                try
                {
                    await deathPersistenceTask;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Death persistence worker failed for user {UserId}", userId);
                }

                room.RemovePlayer(userId);
                if (room.Players.IsEmpty)
                    roomManager.MarkEmpty(room.RoomId);
            }
        });
    }

    private static async Task PersistDeathStatsAsync(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        PlayerDeathEvent evt)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
            await UpsertPlayerStatsAsync(db, evt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert player stats for {VictimId}", evt.VictimId);
        }

        try
        {
            await UpsertLeaderboardAsync(db, evt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert leaderboard for {VictimId}", evt.VictimId);
        }
    }

    private static async Task UpsertPlayerStatsAsync(AppDbContext db, PlayerDeathEvent evt)
    {
        int statsUpdated = await db.PlayerStats
            .Where(ps => ps.UserId == evt.VictimId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(ps => ps.TotalGames, ps => ps.TotalGames + 1)
                .SetProperty(ps => ps.TotalKills, ps => ps.TotalKills + evt.Kills)
                .SetProperty(ps => ps.TotalDeaths, ps => ps.TotalDeaths + 1)
                .SetProperty(ps => ps.BestTerritoryPct,
                    ps => evt.MaxTerritoryPct > ps.BestTerritoryPct
                        ? evt.MaxTerritoryPct
                        : ps.BestTerritoryPct));

        if (statsUpdated != 0)
            return;

        db.PlayerStats.Add(new PlayerStats
        {
            UserId = evt.VictimId,
            TotalGames = 1,
            TotalKills = evt.Kills,
            TotalDeaths = 1,
            BestTerritoryPct = evt.MaxTerritoryPct
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // A concurrent handler inserted the row first - detach and update instead.
            db.ChangeTracker.Clear();
            await db.PlayerStats
                .Where(ps => ps.UserId == evt.VictimId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(ps => ps.TotalGames, ps => ps.TotalGames + 1)
                    .SetProperty(ps => ps.TotalKills, ps => ps.TotalKills + evt.Kills)
                    .SetProperty(ps => ps.TotalDeaths, ps => ps.TotalDeaths + 1)
                    .SetProperty(ps => ps.BestTerritoryPct,
                        ps => evt.MaxTerritoryPct > ps.BestTerritoryPct
                            ? evt.MaxTerritoryPct
                            : ps.BestTerritoryPct));
        }
    }

    private static async Task UpsertLeaderboardAsync(AppDbContext db, PlayerDeathEvent evt)
    {
        // TODO: update Elo based on game performance once Elo calculation is implemented.
        int lbUpdated = await db.Leaderboard
            .Where(lb => lb.UserId == evt.VictimId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(lb => lb.BestPct,
                    lb => evt.MaxTerritoryPct > lb.BestPct
                        ? evt.MaxTerritoryPct
                        : lb.BestPct));

        if (lbUpdated != 0)
            return;

        db.Leaderboard.Add(new Leaderboard
        {
            UserId = evt.VictimId,
            // TODO: set initial Elo once Elo calculation is implemented.
            BestPct = evt.MaxTerritoryPct
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // A concurrent handler inserted the row first - detach and update instead.
            db.ChangeTracker.Clear();
            await db.Leaderboard
                .Where(lb => lb.UserId == evt.VictimId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(lb => lb.BestPct,
                        lb => evt.MaxTerritoryPct > lb.BestPct
                            ? evt.MaxTerritoryPct
                            : lb.BestPct));
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
