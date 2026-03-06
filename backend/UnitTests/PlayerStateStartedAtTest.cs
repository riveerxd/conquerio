using conquerio.Game;

namespace UnitTests;

public class PlayerStateStartedAtTest
{
    [Fact]
    public void StartedAtIsSetToUtc()
    {
        var before = DateTime.UtcNow;
        var p = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            Socket = null!
        };
        var after = DateTime.UtcNow;

        Assert.InRange(p.StartedAt, before, after);
    }
}
