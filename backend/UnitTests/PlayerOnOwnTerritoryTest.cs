using conquerio.Game;

namespace UnitTests;

public class PlayerOnOwnTerritoryTest
{
    [Fact]
    public void NoDeathWhenOnOwnTerritory()
    {
        var grid = new byte[10, 10];
        byte colorId = 1;
        grid[5, 5] = colorId;

        Assert.True(TerritoryResolver.IsOnOwnTerritory(grid, 5, 5, colorId));
    }

    [Fact]
    public void NotOnTerritoryWhenCellBelongsToOther()
    {
        var grid = new byte[10, 10];
        grid[5, 5] = 2;

        Assert.False(TerritoryResolver.IsOnOwnTerritory(grid, 5, 5, 1));
    }

    [Fact]
    public void NotOnTerritoryWhenCellIsEmpty()
    {
        var grid = new byte[10, 10];

        Assert.False(TerritoryResolver.IsOnOwnTerritory(grid, 5, 5, 1));
    }
}
