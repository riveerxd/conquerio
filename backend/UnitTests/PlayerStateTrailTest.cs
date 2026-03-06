using conquerio.Game;

namespace UnitTests;

public class PlayerStateTrailTest
{
    private static PlayerState Create() => new()
    {
        PlayerId = "p1",
        Username = "Alice",
        Socket = null!
    };

    [Fact]
    public void TrailStartsEmpty()
    {
        var p = Create();
        Assert.Empty(p.Trail);
    }

}
