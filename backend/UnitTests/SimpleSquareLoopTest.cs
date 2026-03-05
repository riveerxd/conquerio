using conquerio.Game;

namespace UnitTests;

public class SimpleSquareLoopTest
{
    [Fact]
    public void SquareTrailClaimsEnclosedCells()
    {
        var grid = new byte[10, 10];
        byte colorId = 1;

        grid[2, 2] = colorId;
        grid[2, 3] = colorId;
        grid[2, 4] = colorId;
        grid[2, 5] = colorId;

        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "tester",
            ColorId = colorId,
            Socket = null!,
            Trail = new List<(int X, int Y)>
            {
                (3, 2), (4, 2),
                (4, 3), (4, 4), (4, 5),
                (3, 5)
            }
        };

        var claimed = TerritoryResolver.Resolve(grid, player);
        var claimedSet = new HashSet<(int, int)>(claimed);

        Assert.Contains((3, 3), claimedSet);
        Assert.Contains((3, 4), claimedSet);

        foreach (var (x, y) in player.Trail)
            Assert.Contains((x, y), claimedSet);

        Assert.Equal(8, claimed.Count);
    }
}
