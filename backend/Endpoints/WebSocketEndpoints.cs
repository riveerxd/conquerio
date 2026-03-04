using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
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
            IServiceScopeFactory scopeFactory) =>
        {
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

            using var ws = await context.WebSockets.AcceptWebSocketAsync();

            // join a room
            var room = roomManager.GetOrCreateRoom();
            var player = room.AddPlayer(userId, user.UserName ?? "unknown", ws);

            // persist GameRun + update PlayerStats when this player dies
            // named local function so we can unsubscribe it in the finally block
            async void OnPlayerDied(PlayerDeathEvent evt)
            {
                if (evt.VictimId != userId) return;

                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    await using var tx = await db.Database.BeginTransactionAsync();

                    // save game run (inside the transaction so it rolls back if stats update fails)
                    db.GameRuns.Add(new GameRun
                    {
                        UserId = evt.VictimId,
                        Kills = evt.Kills,
                        MaxTerritoryPct = evt.MaxTerritoryPct,
                        DeathCause = evt.DeathCause,
                        StartedAt = evt.StartedAt,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();

                    // atomically increment player stats to avoid race conditions from concurrent deaths
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

                    if (statsUpdated == 0)
                    {
                        // first run for this player - insert
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
                            // A concurrent handler inserted the row first - detach and update instead
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

                    // atomically upsert leaderboard row
                    // TODO: update Elo based on game performance once Elo calculation is implemented
                    int lbUpdated = await db.Leaderboard
                        .Where(lb => lb.UserId == evt.VictimId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(lb => lb.BestPct,
                                lb => evt.MaxTerritoryPct > lb.BestPct
                                    ? evt.MaxTerritoryPct
                                    : lb.BestPct));

                    if (lbUpdated == 0)
                    {
                        db.Leaderboard.Add(new Leaderboard
                        {
                            UserId = evt.VictimId,
                            // TODO: set initial Elo once Elo calculation is implemented
                            BestPct = evt.MaxTerritoryPct
                        });
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (DbUpdateException)
                        {
                            // A concurrent handler inserted the row first - detach and update instead
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

                    await tx.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Must not let exceptions propagate from async void - they would crash the server.
                    Console.Error.WriteLine($"[PlayerDied] Failed to persist stats for {userId}: {ex}");
                }
            }

            room.PlayerDied += OnPlayerDied;

            // send joined message with full grid
            await MessageSerializer.SendAsync(ws, new JoinedMessage
            {
                PlayerId = userId,
                ColorId = player.ColorId,
                GridWidth = room.GridWidth,
                GridHeight = room.GridHeight,
                TickRate = room.TickRate,
                Grid = room.GetFlatGrid()
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

                        case "ping":
                            await MessageSerializer.SendAsync(ws, new PongMessage { T = msg.T ?? 0 });
                            break;
                    }
                }
            }
            catch (WebSocketException) { } // client disconnected
            finally
            {
                room.PlayerDied -= OnPlayerDied;
                room.RemovePlayer(userId);
            }
        });
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
