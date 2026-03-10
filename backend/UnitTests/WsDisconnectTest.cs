using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsDisconnectTest : WsTestBase
{
    public WsDisconnectTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task Disconnect_PlayerRemovedFromRoom()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"disc_{uid}", $"disc_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        Assert.NotNull(manager.FindRoomForPlayer(playerId));

        await CloseWs(ws);

        await WaitUntil(() => manager.FindRoomForPlayer(playerId) == null);

        Assert.Null(manager.FindRoomForPlayer(playerId));
    }
}
