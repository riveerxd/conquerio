using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class JoinGameAndMoveTest : WsTestBase
{
    public JoinGameAndMoveTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task JoinedMessage_ContainsGridAndPlayer()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"jg_{uid}", $"jg_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);

        Assert.Equal("joined", joined.GetProperty("type").GetString());
        Assert.True(joined.GetProperty("gridWidth").GetInt32() > 0);
        Assert.True(joined.GetProperty("gridHeight").GetInt32() > 0);
        Assert.True(joined.GetProperty("tickRate").GetInt32() > 0);
        Assert.False(string.IsNullOrEmpty(joined.GetProperty("playerId").GetString()));
        // byte[] Grid serializes as base64 string
        Assert.False(string.IsNullOrEmpty(joined.GetProperty("grid").GetString()));

        await CloseWs(ws);
    }

    [Fact]
    public async Task MoveUp_PlayerYDecreases()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"mu_{uid}", $"mu_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];

        await SendMsg(ws, new { type = "input", dir = "up" });
        await WaitUntil(() => player.Direction == Direction.Up);

        var initialY = player.Y;
        room.Tick();

        Assert.True(player.Y < initialY);

        await CloseWs(ws);
    }

    [Fact]
    public async Task MoveLeft_PlayerXDecreases()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"ml_{uid}", $"ml_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];

        // default direction is Right, so Left is opposite — change to Up first
        await SendMsg(ws, new { type = "input", dir = "up" });
        await WaitUntil(() => player.Direction == Direction.Up);
        room.Tick();

        await SendMsg(ws, new { type = "input", dir = "left" });
        await WaitUntil(() => player.Direction == Direction.Left);

        var initialX = player.X;
        room.Tick();

        Assert.True(player.X < initialX);

        await CloseWs(ws);
    }

    [Fact]
    public async Task MoveDown_MultipleTicks_PlayerAdvances()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"md_{uid}", $"md_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];

        await SendMsg(ws, new { type = "input", dir = "down" });
        await WaitUntil(() => player.Direction == Direction.Down);

        var initialY = player.Y;
        for (int i = 0; i < 5; i++)
            room.Tick();

        Assert.Equal(initialY + 5, player.Y);

        await CloseWs(ws);
    }

    [Fact]
    public async Task OppositeDirection_IsIgnored()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"opp_{uid}", $"opp_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];

        // default is Right, sending Left (opposite) should be ignored
        Assert.Equal(Direction.Right, player.Direction);
        await SendMsg(ws, new { type = "input", dir = "left" });
        await Task.Delay(100);
        room.Tick();

        Assert.Equal(Direction.Right, player.Direction);

        await CloseWs(ws);
    }

    [Fact]
    public async Task StateMessage_ContainsPlayerPositions()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"sm_{uid}", $"sm_{uid}@test.com", "Pass123!");

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"state-{uid}");

        using var ws = await ConnectWs(token, room.RoomId);
        var joined = await ReceiveMsg(ws);

        // tick enough for player to leave spawn territory (3x3) and trigger broadcast
        for (int i = 0; i < 5; i++)
            room.Tick();

        var state = await ReceiveMsg(ws);

        Assert.Equal("state", state.GetProperty("type").GetString());
        Assert.True(state.GetProperty("tick").GetInt64() > 0);
        Assert.True(state.GetProperty("players").GetArrayLength() > 0);

        var p = state.GetProperty("players")[0];
        Assert.NotNull(p.GetProperty("id").GetString());
        Assert.True(p.GetProperty("x").GetInt32() >= 0);
        Assert.True(p.GetProperty("y").GetInt32() >= 0);

        await CloseWs(ws);
    }

    [Fact]
    public async Task PlayerTrail_GrowsOutsideTerritory()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"trail_{uid}", $"trail_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.FindRoomForPlayer(playerId)!;
        var player = room.Players[playerId];

        // spawn claims 3x3 territory; move outside by going right several ticks
        // default direction is Right
        for (int i = 0; i < 5; i++)
            room.Tick();

        // player should have moved away from spawn and built a trail
        Assert.True(player.Trail.Count > 0, "trail should grow when outside own territory");

        await CloseWs(ws);
    }
}
