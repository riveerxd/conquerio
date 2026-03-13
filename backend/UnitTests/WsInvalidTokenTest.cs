namespace UnitTests;

public class WsInvalidTokenTest : WsTestBase
{
    public WsInvalidTokenTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task MissingToken_GetsRejected()
    {
        var wsClient = Factory.Server.CreateWebSocketClient();
        await Assert.ThrowsAnyAsync<Exception>(() =>
            wsClient.ConnectAsync(new Uri("ws://localhost/ws/game"), CancellationToken.None));
    }

    [Fact]
    public async Task InvalidToken_GetsRejected()
    {
        var wsClient = Factory.Server.CreateWebSocketClient();
        await Assert.ThrowsAnyAsync<Exception>(() =>
            wsClient.ConnectAsync(
                new Uri("ws://localhost/ws/game?token=not-a-real-jwt"),
                CancellationToken.None));
    }
}
