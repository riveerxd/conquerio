using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using conquerio.Data;
using conquerio.Models;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class LeaderboardTest : WsTestBase
{
    public LeaderboardTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task Leaderboard_EmptyByDefault()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/leaderboard");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
    }

    [Fact]
    public async Task Leaderboard_ReturnsSeededEntries()
    {
        var uid = UniqueId();
        var username = $"lb_{uid}";
        var token = await RegisterAndGetToken(username, $"lb_{uid}@test.com", "Pass123!");

        // get the user ID from /api/auth/me
        var authClient = Factory.Server.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var meRes = await authClient.GetAsync("/api/auth/me");
        var meBody = await meRes.Content.ReadFromJsonAsync<JsonElement>();
        var userId = meBody.GetProperty("id").GetString()!;

        // seed leaderboard entry directly in DB
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Leaderboard.Add(new Leaderboard
            {
                UserId = userId,
                Elo = 1500,
                BestPct = 25.5f
            });
            await db.SaveChangesAsync();
        }

        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/leaderboard");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        bool found = false;
        foreach (var entry in body.EnumerateArray())
        {
            if (entry.GetProperty("userId").GetString() == userId)
            {
                Assert.Equal(1500, entry.GetProperty("elo").GetInt32());
                Assert.True(entry.GetProperty("bestTerritoryPct").GetSingle() > 25f);
                Assert.Equal(username, entry.GetProperty("username").GetString());
                Assert.True(entry.GetProperty("rank").GetInt32() >= 1);
                found = true;
            }
        }
        Assert.True(found, "seeded leaderboard entry should appear");
    }

    [Fact]
    public async Task Leaderboard_OrderedByEloDescending()
    {
        var uid = UniqueId();
        var u1 = $"elo1_{uid}";
        var u2 = $"elo2_{uid}";
        var t1 = await RegisterAndGetToken(u1, $"{u1}@test.com", "Pass123!");
        var t2 = await RegisterAndGetToken(u2, $"{u2}@test.com", "Pass123!");

        // get user IDs
        var authClient = Factory.Server.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t1);
        var me1 = await (await authClient.GetAsync("/api/auth/me")).Content.ReadFromJsonAsync<JsonElement>();
        var id1 = me1.GetProperty("id").GetString()!;

        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t2);
        var me2 = await (await authClient.GetAsync("/api/auth/me")).Content.ReadFromJsonAsync<JsonElement>();
        var id2 = me2.GetProperty("id").GetString()!;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Leaderboard.Add(new Leaderboard { UserId = id1, Elo = 800, BestPct = 10f });
            db.Leaderboard.Add(new Leaderboard { UserId = id2, Elo = 2000, BestPct = 50f });
            await db.SaveChangesAsync();
        }

        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/leaderboard");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        // find both entries
        int rank1 = -1, rank2 = -1;
        foreach (var entry in body.EnumerateArray())
        {
            if (entry.GetProperty("userId").GetString() == id1)
                rank1 = entry.GetProperty("rank").GetInt32();
            if (entry.GetProperty("userId").GetString() == id2)
                rank2 = entry.GetProperty("rank").GetInt32();
        }

        Assert.True(rank2 < rank1, "higher Elo should have lower rank number");
    }

    [Fact]
    public async Task Leaderboard_MaxPlayersParam_LimitsResults()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/leaderboard?maxPlayers=2");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetArrayLength() <= 2);
    }

    [Fact]
    public async Task Leaderboard_LegacyParam_Works()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/leaderboard?max_players=3");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetArrayLength() <= 3);
    }

    [Fact]
    public async Task Leaderboard_NegativeMaxPlayers_ReturnsBadRequest()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/leaderboard?maxPlayers=-1");

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Stats_NonexistentUser_Returns404()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/stats/nonexistent-id-12345");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Stats_SeededData_ReturnsCorrectly()
    {
        var uid = UniqueId();
        var username = $"st_{uid}";
        var token = await RegisterAndGetToken(username, $"st_{uid}@test.com", "Pass123!");

        var authClient = Factory.Server.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var meRes = await authClient.GetAsync("/api/auth/me");
        var meBody = await meRes.Content.ReadFromJsonAsync<JsonElement>();
        var userId = meBody.GetProperty("id").GetString()!;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.PlayerStats.Add(new PlayerStats
            {
                UserId = userId,
                Elo = 1200,
                TotalKills = 5,
                TotalDeaths = 3,
                BestTerritoryPct = 30f,
                TotalGames = 8
            });
            await db.SaveChangesAsync();
        }

        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync($"/api/stats/{userId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(userId, body.GetProperty("userId").GetString());
        Assert.Equal(username, body.GetProperty("username").GetString());
        Assert.Equal(5, body.GetProperty("totalKills").GetInt32());
        Assert.Equal(3, body.GetProperty("totalDeaths").GetInt32());
        Assert.Equal(8, body.GetProperty("totalGames").GetInt32());
    }
}
