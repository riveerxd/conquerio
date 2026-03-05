using conquerio.Game;

namespace conquerio.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET /api/leaderboard
        app.MapGet("/api/leaderboard", () =>
            Results.Ok("Not implemented yet"));

        // GET /api/stats/{id}
        app.MapGet("/api/stats/{id}", (string id) =>
            Results.Ok("Not implemented yet"));

        app.MapGet("/api/rooms", (GameRoomManager roomManager) =>
        {
            var rooms = roomManager.GetAllRooms()
                .Where(r => r.Players.Count > 0)
                .Select(r => new
                {
                    id = r.RoomId,
                    name = r.Name,
                    playerCount = r.Players.Count,
                    maxPlayers = r.MaxPlayers
                });

            return Results.Ok(rooms);
        }).RequireAuthorization();

        app.MapPost("/api/rooms", (GameRoomManager roomManager, CreateRoomRequest? request) =>
        {
            var room = roomManager.CreateRoom(request?.Name);

            return Results.Ok(new
            {
                id = room.RoomId,
                name = room.Name,
                playerCount = 0,
                maxPlayers = room.MaxPlayers
            });
        }).RequireAuthorization();
    }
}

public record CreateRoomRequest(string? Name);
