using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsDisconnectTest : WsTestBase
{
    public WsDisconnectTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task Disconnect_PlayerRemovedFromRoom()
    {
        var token = await RegisterAndGetToken("ws_disc1", "disc1@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        Assert.NotNull(manager.FindRoomForPlayer(playerId));

        await CloseWs(ws);
        await Task.Delay(500);

        Assert.Null(manager.FindRoomForPlayer(playerId));
    }
}
