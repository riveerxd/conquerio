using conquerio.Game;

namespace UnitTests;

public class PlayerStateBoostTest
{
    private static PlayerState Create() => new()
    {
        PlayerId = "p1",
        Username = "Alice",
        Socket = null!
    };

    [Fact]
    public void BoostTicksRemainingDefaultsToZero()
    {
        var p = Create();
        Assert.Equal(0, p.BoostTicksRemaining);
    }

    [Fact]
    public void BoostCooldownTicksRemainingDefaultsToZero()
    {
        var p = Create();
        Assert.Equal(0, p.BoostCooldownTicksRemaining);
    }

}
