using System.Reflection;
using conquerio.Game;

namespace UnitTests;

public class GetDeltaTest
{
    private static readonly MethodInfo GetDeltaMethod =
        typeof(GameRoom).GetMethod("GetDelta", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static (int dx, int dy) Invoke(Direction dir) =>
        ((int, int))GetDeltaMethod.Invoke(null, [dir])!;

    [Theory]
    [InlineData(Direction.Up, 0, -1)]
    [InlineData(Direction.Down, 0, 1)]
    [InlineData(Direction.Left, -1, 0)]
    [InlineData(Direction.Right, 1, 0)]
    public void ReturnsCorrectMovementDelta(Direction dir, int expectedDx, int expectedDy)
    {
        var (dx, dy) = Invoke(dir);

        Assert.Equal(expectedDx, dx);
        Assert.Equal(expectedDy, dy);
    }
}
