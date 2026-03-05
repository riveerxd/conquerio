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

    [Fact]
    public void BoostTicksCanBeSetAndDecremented()
    {
        var p = Create();
        p.BoostTicksRemaining = 60;

        p.BoostTicksRemaining--;

        Assert.Equal(59, p.BoostTicksRemaining);
    }

    [Fact]
    public void BoostCooldownCanBeSetAndDecremented()
    {
        var p = Create();
        p.BoostCooldownTicksRemaining = 200;

        p.BoostCooldownTicksRemaining--;

        Assert.Equal(199, p.BoostCooldownTicksRemaining);
    }
}
