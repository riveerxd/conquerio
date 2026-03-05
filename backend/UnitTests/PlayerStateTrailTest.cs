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

    [Fact]
    public void AddToTrail()
    {
        var p = Create();
        p.Trail.Add((3, 4));
        p.Trail.Add((3, 5));

        Assert.Equal(2, p.Trail.Count);
        Assert.Equal((3, 4), p.Trail[0]);
        Assert.Equal((3, 5), p.Trail[1]);
    }

    [Fact]
    public void ClearTrail()
    {
        var p = Create();
        p.Trail.Add((1, 1));
        p.Trail.Add((2, 2));

        p.Trail.Clear();

        Assert.Empty(p.Trail);
    }
}
