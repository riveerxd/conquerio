using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using conquerio.Game;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class RoomBrowserTest : WsTestBase
{
    public RoomBrowserTest(GameFactory factory) : base(factory) { }

    private async Task<HttpClient> AuthenticatedClient()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"rb_{uid}", $"rb_{uid}@test.com", "Pass123!");
        var client = Factory.Server.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetRooms_Unauthorized_Returns401()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/rooms");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task GetRooms_EmptyRooms_ReturnsEmptyArray()
    {
        using var client = await AuthenticatedClient();
        var res = await client.GetAsync("/api/rooms");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var rooms = await res.Content.ReadFromJsonAsync<JsonElement>();
        // only rooms with players are returned, so this should be an array
        Assert.Equal(JsonValueKind.Array, rooms.ValueKind);
    }

    [Fact]
    public async Task CreateRoom_ReturnsRoomInfo()
    {
        using var client = await AuthenticatedClient();
        var res = await client.PostAsJsonAsync("/api/rooms", new { });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("id").GetString()));
        Assert.False(string.IsNullOrEmpty(body.GetProperty("name").GetString()));
        Assert.Equal(0, body.GetProperty("playerCount").GetInt32());
        Assert.True(body.GetProperty("maxPlayers").GetInt32() > 0);
    }

    [Fact]
    public async Task CreateRoom_WithName_PreservesName()
    {
        using var client = await AuthenticatedClient();
        var uid = UniqueId();
        var name = $"my-room-{uid}";
        var res = await client.PostAsJsonAsync("/api/rooms", new { name });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(name, body.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetRooms_ShowsRoomWithPlayers()
    {
        var uid = UniqueId();
        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"browse-{uid}");
        room.AddPlayer($"browse-player-{uid}", $"Player{uid}", null!);

        try
        {
            using var client = await AuthenticatedClient();
            var res = await client.GetAsync("/api/rooms");
            var rooms = await res.Content.ReadFromJsonAsync<JsonElement>();

            bool found = false;
            foreach (var r in rooms.EnumerateArray())
            {
                if (r.GetProperty("id").GetString() == room.RoomId)
                {
                    Assert.Equal(1, r.GetProperty("playerCount").GetInt32());
                    found = true;
                }
            }
            Assert.True(found, "created room should appear in room list");
        }
        finally
        {
            room.RemovePlayer($"browse-player-{uid}");
            manager.RemoveRoom(room.RoomId);
        }
    }

    [Fact]
    public async Task GetRooms_HidesEmptyRooms()
    {
        var uid = UniqueId();
        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"empty-{uid}");

        try
        {
            using var client = await AuthenticatedClient();
            var res = await client.GetAsync("/api/rooms");
            var rooms = await res.Content.ReadFromJsonAsync<JsonElement>();

            foreach (var r in rooms.EnumerateArray())
                Assert.NotEqual(room.RoomId, r.GetProperty("id").GetString());
        }
        finally
        {
            manager.RemoveRoom(room.RoomId);
        }
    }

    [Fact]
    public async Task JoinSpecificRoom_ViaWebSocket()
    {
        var uid = UniqueId();
        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        var room = manager.CreateRoom($"join-{uid}");

        var token = await RegisterAndGetToken($"join_{uid}", $"join_{uid}@test.com", "Pass123!");
        using var ws = await ConnectWs(token, room.RoomId);
        var joined = await ReceiveMsg(ws);

        Assert.Equal("joined", joined.GetProperty("type").GetString());
        var playerId = joined.GetProperty("playerId").GetString()!;

        var foundRoom = manager.FindRoomForPlayer(playerId);
        Assert.NotNull(foundRoom);
        Assert.Equal(room.RoomId, foundRoom.RoomId);

        await CloseWs(ws);
    }

    [Fact]
    public async Task QuickPlay_AssignsRoom()
    {
        var uid = UniqueId();
        var token = await RegisterAndGetToken($"qp_{uid}", $"qp_{uid}@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var joined = await ReceiveMsg(ws);

        Assert.Equal("joined", joined.GetProperty("type").GetString());
        var playerId = joined.GetProperty("playerId").GetString()!;

        var manager = Factory.Services.GetRequiredService<GameRoomManager>();
        Assert.NotNull(manager.FindRoomForPlayer(playerId));

        await CloseWs(ws);
    }
}
