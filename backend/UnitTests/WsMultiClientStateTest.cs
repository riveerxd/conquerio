using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsMultiClientStateTest : WsTestBase
{
    public WsMultiClientStateTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task MultipleClients_ReceiveStateUpdates()
    {
        var uid = UniqueId();
        var token1 = await RegisterAndGetToken($"multi1_{uid}", $"multi1_{uid}@test.com", "Pass123!");
        var token2 = await RegisterAndGetToken($"multi2_{uid}", $"multi2_{uid}@test.com", "Pass123!");

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"multi-room-{uid}");

        using var ws1 = await ConnectWs(token1, room.RoomId);
        var joined1 = await ReceiveMsg(ws1);
        Assert.Equal("joined", joined1.GetProperty("type").GetString());

        using var ws2 = await ConnectWs(token2, room.RoomId);
        var joined2 = await ReceiveMsg(ws2);
        Assert.Equal("joined", joined2.GetProperty("type").GetString());

        for (int i = 0; i < 5; i++)
            room.Tick();

        var state1 = await ReceiveMsg(ws1);
        var state2 = await ReceiveMsg(ws2);

        Assert.Equal("state", state1.GetProperty("type").GetString());
        Assert.Equal("state", state2.GetProperty("type").GetString());

        Assert.Equal(2, state1.GetProperty("players").GetArrayLength());
        Assert.Equal(2, state2.GetProperty("players").GetArrayLength());

        await CloseWs(ws1);
        await CloseWs(ws2);
    }
}
