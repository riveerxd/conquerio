using conquerio.Game;

namespace UnitTests;

public class HitsSelfTrailTest
{
    [Fact]
    public void DetectsHitOnOwnTrail()
    {
        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            ColorId = 1,
            Socket = null!,
            Trail = new List<(int X, int Y)> { (2, 2), (3, 2), (4, 2), (5, 2) }
        };

        Assert.True(CollisionDetector.HitsSelfTrail(3, 2, player));
    }

    [Fact]
    public void IgnoresLastTrailPointAsCurrentHead()
    {
        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            ColorId = 1,
            Socket = null!,
            Trail = new List<(int X, int Y)> { (2, 2), (3, 2), (4, 2) }
        };

        Assert.False(CollisionDetector.HitsSelfTrail(4, 2, player));
    }

    [Fact]
    public void ReturnsFalseWhenTrailIsEmpty()
    {
        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            ColorId = 1,
            Socket = null!,
            Trail = new List<(int X, int Y)>()
        };

        Assert.False(CollisionDetector.HitsSelfTrail(0, 0, player));
    }
}
