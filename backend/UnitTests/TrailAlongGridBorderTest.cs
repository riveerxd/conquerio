using conquerio.Game;

namespace UnitTests;

public class TrailAlongGridBorderTest
{
    [Fact]
    public void TrailAlongEdgesEnclosesInteriorCells()
    {
        var grid = new byte[10, 10];
        byte colorId = 1;

        grid[0, 2] = colorId;

        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "tester",
            ColorId = colorId,
            Socket = null!,
            Trail = new List<(int X, int Y)>
            {
                (0, 1), (0, 0),
                (1, 0), (2, 0), (3, 0),
                (3, 1), (3, 2),
                (2, 2), (1, 2)
            }
        };

        var claimed = TerritoryResolver.Resolve(grid, player);
        var claimedSet = new HashSet<(int, int)>(claimed);

        Assert.Contains((1, 1), claimedSet);
        Assert.Contains((2, 1), claimedSet);

        foreach (var (x, y) in player.Trail)
            Assert.Contains((x, y), claimedSet);

        Assert.Equal(9 + 2, claimed.Count);
    }
}
