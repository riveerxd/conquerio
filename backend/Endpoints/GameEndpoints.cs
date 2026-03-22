using conquerio.Data;
using conquerio.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace conquerio.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET /api/leaderboard?maxPlayers={n}
        // Backwards compatible with max_players.
        app.MapGet("/api/leaderboard", async (
            AppDbContext db,
            int? maxPlayers,
            [FromQuery(Name = "max_players")] int? maxPlayersLegacy) =>
        {
            const int defaultMaxPlayers = 10;
            const int maxPlayersCap = 100;

            int requested = maxPlayers ?? maxPlayersLegacy ?? defaultMaxPlayers;
            if (requested <= 0)
                return Results.BadRequest("maxPlayers must be a positive integer.");

            int take = Math.Min(requested, maxPlayersCap);

            var entries = await db.Leaderboard
                .Include(lb => lb.User)
                .OrderByDescending(lb => lb.Elo)
                .Take(take)
                .Select(lb => new
                {
                    UserId = lb.UserId,
                    Username = lb.User.UserName,
                    Elo = lb.Elo,
                    BestTerritoryPct = lb.BestPct
                })
                .ToListAsync();

            return Results.Ok(entries.Select((e, i) => new
            {
                Rank = i + 1,
                e.UserId,
                e.Username,
                e.Elo,
                e.BestTerritoryPct
            }));
        })
        .WithTags("Game")
        .WithSummary("Get leaderboard entries")
        .WithDescription("Retrieves the top players by Elo rating, limited by the maxPlayers parameter.");

        // GET /api/stats/{id}
        app.MapGet("/api/stats/{id}", async (string id, AppDbContext db) =>
        {
            var stats = await db.PlayerStats
                .Include(ps => ps.User)
                .Where(ps => ps.UserId == id)
                .Select(ps => new
                {
                    ps.UserId,
                    Username = ps.User.UserName,
                    ps.Elo,
                    ps.TotalKills,
                    ps.TotalDeaths,
                    ps.BestTerritoryPct,
                    ps.TotalGames
                })
                .FirstOrDefaultAsync();

            return stats is null
                ? Results.NotFound($"No stats found for player '{id}'.")
                : Results.Ok(stats);
        })
        .WithTags("Game")
        .WithSummary("Get player statistics")
        .WithDescription("Returns game statistics for a specific player by their user ID.");

        app.MapGet("/api/rooms", (GameRoomManager roomManager) =>
        {
            var rooms = roomManager.GetAllRooms()
                .Where(r => r.Players.Count > 0)
                .Select(r => new
                {
                    id = r.RoomId,
                    name = r.Name,
                    playerCount = r.Players.Count,
                    maxPlayers = r.MaxPlayers,
                    gridSize = r.GridWidth <= 100 ? "small" : r.GridWidth >= 300 ? "large" : "medium",
                    abilitiesEnabled = r.AbilitiesEnabled,
                    isPrivate = r.JoinCode != null
                });

            return Results.Ok(rooms);
        })
        .RequireAuthorization()
        .WithTags("Game")
        .WithSummary("List active game rooms")
        .WithDescription("Lists all current game rooms that have players.");

        app.MapPost("/api/rooms", (GameRoomManager roomManager, CreateRoomRequest? request) =>
        {
            var (gridWidth, gridHeight) = ParseGridSize(request?.GridSize);
            var maxPlayers = Math.Clamp(request?.MaxPlayers ?? 20, 2, 100);
            var settings = new RoomSettings
            {
                GridWidth = gridWidth,
                GridHeight = gridHeight,
                MaxPlayers = maxPlayers,
                AbilitiesEnabled = request?.AbilitiesEnabled ?? true,
                JoinCode = string.IsNullOrWhiteSpace(request?.JoinCode) ? null : request.JoinCode
            };
            var room = roomManager.CreateRoom(request?.Name, settings);

            return Results.Ok(new
            {
                id = room.RoomId,
                name = room.Name,
                playerCount = 0,
                maxPlayers = room.MaxPlayers,
                gridSize = room.GridWidth <= 100 ? "small" : room.GridWidth >= 300 ? "large" : "medium",
                abilitiesEnabled = room.AbilitiesEnabled,
                isPrivate = room.JoinCode != null
            });
        })
        .RequireAuthorization()
        .WithTags("Game")
        .WithSummary("Create a new game room")
        .WithDescription("Manually creates a new game room where players can join.");
    }

    private static (int Width, int Height) ParseGridSize(string? gridSize) => gridSize?.ToLowerInvariant() switch
    {
        "small" => (100, 100),
        "large" => (300, 300),
        _ => (200, 200)
    };
}

record CreateRoomRequest(string? Name, string? GridSize, int? MaxPlayers, bool? AbilitiesEnabled, string? JoinCode);

