using conquerio.Game;

namespace UnitTests;

public class LShapedTerritoryTest
{
    [Fact]
    public void LShapedTrailClaimsLShapedInterior()
    {
        var grid = new byte[10, 10];
        byte colorId = 1;

        for (int y = 1; y <= 7; y++)
            grid[1, y] = colorId;

        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "tester",
            ColorId = colorId,
            Socket = null!,
            Trail = new List<(int X, int Y)>
            {
                (2, 1), (3, 1), (4, 1),
                (4, 2), (4, 3),
                (5, 3), (6, 3),
                (6, 4), (6, 5), (6, 6), (6, 7),
                (5, 7), (4, 7), (3, 7), (2, 7)
            }
        };

        var claimed = TerritoryResolver.Resolve(grid, player);
        var claimedSet = new HashSet<(int, int)>(claimed);

        var expectedInterior = new List<(int, int)>
        {
            (2, 2), (3, 2),
            (2, 3), (3, 3),
            (2, 4), (3, 4), (4, 4), (5, 4),
            (2, 5), (3, 5), (4, 5), (5, 5),
            (2, 6), (3, 6), (4, 6), (5, 6)
        };

        foreach (var cell in expectedInterior)
            Assert.Contains(cell, claimedSet);

        foreach (var (x, y) in player.Trail)
            Assert.Contains((x, y), claimedSet);

        Assert.Equal(15 + 16, claimed.Count);
    }
}
