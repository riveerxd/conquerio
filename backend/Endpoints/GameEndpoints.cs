using conquerio.Data;
using Microsoft.EntityFrameworkCore;

namespace conquerio.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET /api/leaderboard?max_players={n}
        app.MapGet("/api/leaderboard", async (AppDbContext db, int max_players = 10) =>
        {
            if (max_players <= 0)
                return Results.BadRequest("max_players must be a positive integer.");

            var entries = await db.Leaderboard
                .Include(lb => lb.User)
                .OrderByDescending(lb => lb.Elo)
                .Take(max_players)
                .Select(lb => new
                {
                    Rank = 0, // will be assigned below
                    UserId = lb.UserId,
                    Username = lb.User.UserName,
                    Elo = lb.Elo,
                    BestTerritoryPct = lb.BestPct
                })
                .ToListAsync();

            var ranked = entries
                .Select((e, i) => new
                {
                    Rank = i + 1,
                    e.UserId,
                    e.Username,
                    e.Elo,
                    e.BestTerritoryPct
                });

            return Results.Ok(ranked);
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


