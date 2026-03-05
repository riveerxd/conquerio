using conquerio.Game;

namespace UnitTests;

public class PlayerStateStatsTest
{
    private static PlayerState Create() => new()
    {
        PlayerId = "p1",
        Username = "Alice",
        Socket = null!
    };

    [Fact]
    public void OwnedCellsCanBeTracked()
    {
        var p = Create();
        Assert.Equal(0, p.OwnedCells);

        p.OwnedCells = 25;
        Assert.Equal(25, p.OwnedCells);
    }

    [Fact]
    public void MaxTerritoryPctCanBeTracked()
    {
        var p = Create();
        Assert.Equal(0f, p.MaxTerritoryPct);

        p.MaxTerritoryPct = 12.5f;
        Assert.Equal(12.5f, p.MaxTerritoryPct);
    }
}
