using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsDirectionInputTest : WsTestBase
{
    public WsDirectionInputTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task DirectionInput_PlayerMovesOnTick()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"dir_{uid}", $"dir_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];
        var initialY = player.Y;

        await SendMsg(ws, new { type = "input", dir = "down" });

        await WaitUntil(() => player.Direction == Direction.Down);

        for (int i = 0; i < 5; i++)
            room.Tick();

        Assert.Equal(Direction.Down, player.Direction);
        Assert.True(player.Y > initialY);

        await CloseWs(ws);
    }
}
