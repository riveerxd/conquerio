using System.Text.Json;
using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class WsUsernameInStateTest : WsTestBase
{
    public WsUsernameInStateTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task StateMessage_IncludesCorrectUsername()
    {
        var uid = UniqueId();
        var username = $"alice_{uid}";
        var token = await RegisterAndGetToken(username, $"{username}@test.com", "Pass123!");

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"username-test-room-{uid}");

        using var ws = await ConnectWs(token, room.RoomId);
        var joined = await ReceiveMsg(ws);
        Assert.Equal("joined", joined.GetProperty("type").GetString());

        // Tick until we get a state message
        JsonElement? stateMsg = null;
        for (int i = 0; i < 10 && stateMsg is null; i++)
        {
            room.Tick();
            try
            {
                var msg = await ReceiveMsg(ws, timeoutMs: 500);
                if (msg.GetProperty("type").GetString() == "state")
                    stateMsg = msg;
            }
            catch (OperationCanceledException) { }
        }

        Assert.NotNull(stateMsg);

        var players = stateMsg.Value.GetProperty("players");
        Assert.True(players.GetArrayLength() > 0, "Expected at least one player in state");

        var player = players[0];
        Assert.True(player.TryGetProperty("username", out var usernameEl),
            "PlayerDto must include 'username' field");

        var receivedUsername = usernameEl.GetString();
        Assert.Equal(username, receivedUsername);

        await CloseWs(ws);
    }
}
