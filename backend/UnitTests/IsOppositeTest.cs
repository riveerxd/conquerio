using System.Reflection;
using conquerio.Game;

namespace UnitTests;

public class IsOppositeTest
{
    private static readonly MethodInfo IsOppositeMethod =
        typeof(GameRoom).GetMethod("IsOpposite", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static bool Invoke(Direction a, Direction b) =>
        (bool)IsOppositeMethod.Invoke(null, [a, b])!;

    [Theory]
    [InlineData(Direction.Up, Direction.Down)]
    [InlineData(Direction.Down, Direction.Up)]
    [InlineData(Direction.Left, Direction.Right)]
    [InlineData(Direction.Right, Direction.Left)]
    public void DetectsOppositeDirections(Direction a, Direction b)
    {
        Assert.True(Invoke(a, b));
    }

    [Theory]
    [InlineData(Direction.Up, Direction.Left)]
    [InlineData(Direction.Up, Direction.Right)]
    [InlineData(Direction.Up, Direction.Up)]
    [InlineData(Direction.Down, Direction.Left)]
    public void ReturnsFalseForNonOppositeDirections(Direction a, Direction b)
    {
        Assert.False(Invoke(a, b));
    }
}
