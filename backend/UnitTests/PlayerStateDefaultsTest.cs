using conquerio.Game;

namespace UnitTests;

public class PlayerStateDefaultsTest
{
    private static PlayerState Create() => new()
    {
        PlayerId = "p1",
        Username = "Alice",
        Socket = null!
    };

    [Fact]
    public void IsAliveDefaultsToTrue()
    {
        var p = Create();
        Assert.True(p.IsAlive);
    }

    [Fact]
    public void DirectionDefaultsToRight()
    {
        var p = Create();
        Assert.Equal(Direction.Right, p.Direction);
    }

    [Fact]
    public void SpeedMultiplierDefaultsToOne()
    {
        var p = Create();
        Assert.Equal(1.0f, p.SpeedMultiplier);
    }
}
