using conquerio.Data;
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
        });

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
        });
    }
}


