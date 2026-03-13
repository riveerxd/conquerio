using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UnitTests;

public class AuthRegisterLoginTest : WsTestBase
{
    public AuthRegisterLoginTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ReturnsSuccess()
    {
        var uid = UniqueId();
        var client = Factory.Server.CreateClient();

        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"reg_{uid}",
            email = $"reg_{uid}@test.com",
            password = "Pass123!"
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Registration successful.", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        var uid = UniqueId();
        var client = Factory.Server.CreateClient();
        var payload = new
        {
            username = $"dup_{uid}",
            email = $"dup_{uid}@test.com",
            password = "Pass123!"
        };

        await client.PostAsJsonAsync("/api/auth/register", payload);
        var res = await client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        var uid = UniqueId();
        var client = Factory.Server.CreateClient();

        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"weak_{uid}",
            email = $"weak_{uid}@test.com",
            password = "12"
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var uid = UniqueId();
        var client = Factory.Server.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"login_{uid}",
            email = $"login_{uid}@test.com",
            password = "Pass123!"
        });

        var res = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = $"login_{uid}",
            password = "Pass123!"
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString();
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var uid = UniqueId();
        var client = Factory.Server.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"bad_{uid}",
            email = $"bad_{uid}@test.com",
            password = "Pass123!"
        });

        var res = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = $"bad_{uid}",
            password = "WrongPassword1!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Login_NonexistentUser_ReturnsUnauthorized()
    {
        var uid = UniqueId();
        var client = Factory.Server.CreateClient();

        var res = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = $"nouser_{uid}",
            password = "Pass123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task AuthMe_WithValidToken_ReturnsUserInfo()
    {
        var uid = UniqueId();
        var username = $"me_{uid}";
        var token = await RegisterAndGetToken(username, $"me_{uid}@test.com", "Pass123!");

        var client = Factory.Server.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(username, body.GetProperty("username").GetString());
    }

    [Fact]
    public async Task AuthMe_WithoutToken_ReturnsUnauthorized()
    {
        var client = Factory.Server.CreateClient();
        var res = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
