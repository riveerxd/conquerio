using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsFullRoomTest : WsTestBase
{
    public WsFullRoomTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task FullRoom_GetsRejected()
    {
        var uid = UniqueId();
        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"full-room-{uid}");

        // fill with fake players
        for (int i = 0; i < room.MaxPlayers; i++)
            room.AddPlayer($"fake-{uid}-{i}", $"Fake{i}", null!);

        Assert.True(room.IsFull);

        try
        {
            var token = await RegisterAndGetToken($"full_{uid}", $"full_{uid}@test.com", "Pass123!");
            var wsClient = Factory.Server.CreateWebSocketClient();
            var uri = new Uri(
                $"ws://localhost/ws/game?token={Uri.EscapeDataString(token)}&roomId={room.RoomId}");

            await Assert.ThrowsAnyAsync<Exception>(() =>
                wsClient.ConnectAsync(uri, CancellationToken.None));
        }
        finally
        {
            // cleanup fake players so they don't interfere with other tests
            foreach (var pid in room.Players.Keys.ToList())
                room.RemovePlayer(pid);
            manager.RemoveRoom(room.RoomId);
        }
    }
}
