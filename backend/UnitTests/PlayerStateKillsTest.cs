using conquerio.Game;

namespace UnitTests;

public class PlayerStateKillsTest
{
    [Fact]
    public void KillsStartsAtZero()
    {
        var p = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            Socket = null!
        };

        Assert.Equal(0, p.Kills);
    }

    [Fact]
    public void KillsIncrementCorrectly()
    {
        var p = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            Socket = null!
        };

        p.Kills++;
        p.Kills++;
        p.Kills++;

        Assert.Equal(3, p.Kills);
    }
}
