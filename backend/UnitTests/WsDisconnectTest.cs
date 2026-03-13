using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsDisconnectTest : WsTestBase
{
    public WsDisconnectTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task Disconnect_PlayerMarkedDisconnected()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"disc_{uid}", $"disc_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId);
        Assert.NotNull(room);
        Assert.False(room!.Players[playerId].IsDisconnected);

        await CloseWs(ws);

        // player gets marked disconnected (not removed - there's a grace period)
        await WaitUntil(() => room.Players[playerId].IsDisconnected);

        Assert.True(room.Players[playerId].IsDisconnected);
    }
}
