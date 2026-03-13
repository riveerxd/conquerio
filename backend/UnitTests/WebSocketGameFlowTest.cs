using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using conquerio.Data;
using conquerio.Endpoints;
using conquerio.Game;
using conquerio.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace UnitTests;

public class GameFactory : IAsyncLifetime
{
    private WebApplication _app = null!;
    private SqliteConnection _connection = null!;

    public TestServer Server => _app.GetTestServer();
    public IServiceProvider Services => _app.Services;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_connection));

        builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        const string jwtKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm!";
        builder.Configuration["JwtSettings:SecretKey"] = jwtKey;
        builder.Configuration["JwtSettings:Issuer"] = "conquerio";
        builder.Configuration["JwtSettings:Audience"] = "conquerio-users";

        var key = Encoding.UTF8.GetBytes(jwtKey);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "conquerio",
                ValidAudience = "conquerio-users",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<GameRoomManager>();

        _app = builder.Build();

        using (var scope = _app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        _app.UseWebSockets();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapAuthEndpoints();
        _app.MapGameEndpoints();
        _app.MapWebSocketEndpoints();
        _app.MapHealthEndpoints();

        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

public abstract class WsTestBase : IClassFixture<GameFactory>
{
    protected readonly GameFactory Factory;
    private static int _userCounter;

    protected WsTestBase(GameFactory factory)
    {
        Factory = factory;
    }

    protected static string UniqueId() => $"{Interlocked.Increment(ref _userCounter)}_{Guid.NewGuid():N}";

    protected async Task<string> RegisterAndGetToken(string username, string email, string password)
    {
        var client = Factory.Server.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register",
            new { username, email, password });
        regResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new { username, password });
        loginResponse.EnsureSuccessStatusCode();

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    protected async Task<WebSocket> ConnectWs(string token, string? roomId = null)
    {
        var wsClient = Factory.Server.CreateWebSocketClient();
        var uri = $"ws://localhost/ws/game?token={Uri.EscapeDataString(token)}";
        if (roomId != null) uri += $"&roomId={Uri.EscapeDataString(roomId)}";
        return await wsClient.ConnectAsync(new Uri(uri), CancellationToken.None);
    }

    protected static async Task<JsonElement> ReceiveMsg(WebSocket ws, int timeoutMs = 5000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        var buffer = new byte[8192];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await ws.ReceiveAsync(buffer, cts.Token);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        using var doc = JsonDocument.Parse(ms.ToArray());
        return doc.RootElement.Clone();
    }

    protected static async Task SendMsg(WebSocket ws, object message)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(message,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await ws.SendAsync(json, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    protected static async Task CloseWs(WebSocket ws)
    {
        if (ws.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    protected static async Task WaitUntil(Func<bool> condition, int timeoutMs = 2000, int pollMs = 20)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition() && DateTime.UtcNow < deadline)
            await Task.Delay(pollMs);
    }
}
