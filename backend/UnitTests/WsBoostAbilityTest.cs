using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsBoostAbilityTest : WsTestBase
{
    public WsBoostAbilityTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task BoostAbility_SpeedChanges()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"boost_{uid}", $"boost_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];

        Assert.Equal(1f, player.SpeedMultiplier);

        await SendMsg(ws, new { type = "ability", ability = "BOOST" });

        await WaitUntil(() => player.PendingAbility == "BOOST");

        room.Tick();

        Assert.Equal(2f, player.SpeedMultiplier);

        await CloseWs(ws);
    }
}
