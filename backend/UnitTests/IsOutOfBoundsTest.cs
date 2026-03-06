using conquerio.Game;

namespace UnitTests;

public class IsOutOfBoundsTest
{
    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    [InlineData(100, 5)]
    [InlineData(5, 100)]
    public void DetectsOutOfBounds(int x, int y)
    {
        Assert.True(CollisionDetector.IsOutOfBounds(x, y, 100, 100));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(50, 50)]
    [InlineData(99, 99)]
    public void ReturnsFalseForValidPositions(int x, int y)
    {
        Assert.False(CollisionDetector.IsOutOfBounds(x, y, 100, 100));
    }
}
