namespace UnitTests;

public class WsValidJwtTest : WsTestBase
{
    public WsValidJwtTest(GameFactory factory) : base(factory) { }

    [Fact]
    public async Task ValidJwt_ReceivesJoinedMessage()
    {
        var token = await RegisterAndGetToken("ws_jwt1", "jwt1@test.com", "Pass123!");

        using var ws = await ConnectWs(token);
        var msg = await ReceiveMsg(ws);

        Assert.Equal("joined", msg.GetProperty("type").GetString());
        Assert.Equal(200, msg.GetProperty("gridWidth").GetInt32());
        Assert.Equal(200, msg.GetProperty("gridHeight").GetInt32());
        Assert.Equal(20, msg.GetProperty("tickRate").GetInt32());
        Assert.NotNull(msg.GetProperty("playerId").GetString());
        Assert.True(msg.GetProperty("colorId").GetByte() > 0);

        await CloseWs(ws);
    }
}
