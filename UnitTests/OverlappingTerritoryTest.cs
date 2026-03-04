using conquerio.Game;

namespace UnitTests;

public class OverlappingTerritoryTest
{
    [Fact]
    public void EnclosedCellsOwnedByOtherPlayerAreClaimed()
    {
        var grid = new byte[10, 10];
        byte player1Color = 1;
        byte player2Color = 2;

        for (int y = 1; y <= 5; y++)
            grid[1, y] = player1Color;

        grid[2, 3] = player2Color;
        grid[3, 3] = player2Color;

        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "tester",
            ColorId = player1Color,
            Socket = null!,
            Trail = new List<(int X, int Y)>
            {
                (2, 1), (3, 1), (4, 1),
                (4, 2), (4, 3), (4, 4), (4, 5),
                (3, 5), (2, 5)
            }
        };

        var claimed = TerritoryResolver.Resolve(grid, player);
        var claimedSet = new HashSet<(int, int)>(claimed);

        Assert.Contains((2, 3), claimedSet);
        Assert.Contains((3, 3), claimedSet);

        var expectedInterior = new List<(int, int)>
        {
            (2, 2), (3, 2),
            (2, 3), (3, 3),
            (2, 4), (3, 4)
        };

        foreach (var cell in expectedInterior)
            Assert.Contains(cell, claimedSet);

        foreach (var (x, y) in player.Trail)
            Assert.Contains((x, y), claimedSet);

        Assert.Equal(9 + 6, claimed.Count);
    }
}
