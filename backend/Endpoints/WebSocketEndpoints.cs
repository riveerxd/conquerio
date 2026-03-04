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
            room.PlayerDied += async (evt) =>
            {
                if (evt.VictimId != userId) return;

                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // save game run
                db.GameRuns.Add(new GameRun
                {
                    UserId = evt.VictimId,
                    Kills = evt.Kills,
                    MaxTerritoryPct = evt.MaxTerritoryPct,
                    DeathCause = evt.DeathCause,
                    StartedAt = evt.StartedAt,
                    UpdatedAt = DateTime.UtcNow
                });

                // upsert player stats
                var stats = await db.PlayerStats.FindAsync(evt.VictimId);
                if (stats == null)
                {
                    stats = new PlayerStats { UserId = evt.VictimId };
                    db.PlayerStats.Add(stats);
                }

                stats.TotalGames++;
                stats.TotalKills += evt.Kills;
                stats.TotalDeaths++;
                if (evt.MaxTerritoryPct > stats.BestTerritoryPct)
                    stats.BestTerritoryPct = evt.MaxTerritoryPct;

                // upsert leaderboard row (mirrors Elo from stats)
                var lb = await db.Leaderboard.FindAsync(evt.VictimId);
                if (lb == null)
                {
                    lb = new Leaderboard { UserId = evt.VictimId, Elo = stats.Elo };
                    db.Leaderboard.Add(lb);
                }
                else
                {
                    lb.Elo = stats.Elo;
                    lb.BestPct = stats.BestTerritoryPct;
                }

                await db.SaveChangesAsync();
            };

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
