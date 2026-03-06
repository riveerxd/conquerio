using conquerio.Game;

namespace UnitTests;

public class HitsTrailTest
{
    [Fact]
    public void DetectsHitOnAnotherPlayersTrail()
    {
        var other = new PlayerState
        {
            PlayerId = "p2",
            Username = "Bob",
            ColorId = 2,
            Socket = null!,
            Trail = new List<(int X, int Y)> { (5, 5), (6, 5), (7, 5) }
        };

        Assert.True(CollisionDetector.HitsTrail(6, 5, other));
    }

    [Fact]
    public void ReturnsFalseWhenNotOnTrail()
    {
        var other = new PlayerState
        {
            PlayerId = "p2",
            Username = "Bob",
            ColorId = 2,
            Socket = null!,
            Trail = new List<(int X, int Y)> { (5, 5), (6, 5), (7, 5) }
        };

        Assert.False(CollisionDetector.HitsTrail(10, 10, other));
    }

    [Fact]
    public void ExcludesPlayerByIdInMultiPlayerOverload()
    {
        var p1 = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            ColorId = 1,
            Socket = null!,
            Trail = new List<(int X, int Y)> { (3, 3) }
        };

        var p2 = new PlayerState
        {
            PlayerId = "p2",
            Username = "Bob",
            ColorId = 2,
            Socket = null!,
            Trail = new List<(int X, int Y)> { (7, 7) }
        };

        Assert.False(CollisionDetector.HitsTrail(3, 3, new[] { p1, p2 }, "p1"));
        Assert.True(CollisionDetector.HitsTrail(7, 7, new[] { p1, p2 }, "p1"));
    }
}
