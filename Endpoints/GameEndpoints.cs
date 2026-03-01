namespace conquerio.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET /api/leaderboard
        app.MapGet("/api/leaderboard", () =>
            Results.Ok("Not implemented yet"));

        // GET /api/stats/{id}
        app.MapGet("/api/stats/{id}", (string _) =>
            Results.Ok("Not implemented yet"));
    }
}


