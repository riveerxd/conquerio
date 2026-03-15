using conquerio.Game;
using conquerio.Game.Abilities;

namespace UnitTests;

public class PlayerAbilityTest
{
    private static (GameRoom room, PlayerState player, BoostAbility ability) Setup()
    {
        var room = new GameRoom("test", "test");
        var player = new PlayerState
        {
            PlayerId = "p1",
            Username = "Alice",
            Socket = null!
        };
        var ability = new BoostAbility(room, player);
        return (room, player, ability);
    }

    [Fact]
    public void IsReadyByDefault()
    {
        var (_, _, ability) = Setup();
        Assert.True(ability.IsReady);
    }

    [Fact]
    public void Activate_SetsIsActivated()
    {
        var (_, _, ability) = Setup();
        ability.Activate();
        Assert.True(ability.IsActivated);
    }

    [Fact]
    public void Activate_IsReadyFalseWhileActive()
    {
        var (_, _, ability) = Setup();
        ability.Activate();
        Assert.False(ability.IsReady);
    }

    [Fact]
    public void Activate_WhenOnCooldown_DoesNothing()
    {
        var (room, _, ability) = Setup();
        ability.Activate();
        // tick through until ability expires and cooldown starts
        for (int i = 0; i < room.TickRate * 10; i++)
            ability.Tick();
        // now on cooldown
        Assert.False(ability.IsReady);
        var cooldownBefore = ability.CooldownTicksRemaining;
        ability.Activate();
        // cooldown should not have changed - Activate was a no-op
        Assert.Equal(cooldownBefore, ability.CooldownTicksRemaining);
        Assert.False(ability.IsActivated);
    }

    [Fact]
    public void Tick_DecrementsActiveDuration()
    {
        var (_, _, ability) = Setup();
        ability.Activate();
        var before = ability.DurationTicksRemaining;
        ability.Tick();
        Assert.Equal(before - 1, ability.DurationTicksRemaining);
    }

    [Fact]
    public void Tick_AfterDurationExpires_StartsCooldown()
    {
        var (room, _, ability) = Setup();
        ability.Activate();
        for (int i = 0; i < room.TickRate * 10; i++)
            ability.Tick();
        Assert.False(ability.IsActivated);
        Assert.True(ability.CooldownTicksRemaining > 0);
    }

    [Fact]
    public void Tick_AfterCooldownExpires_IsReady()
    {
        var (room, _, ability) = Setup();
        ability.Activate();
        // tick through duration + cooldown
        for (int i = 0; i < room.TickRate * 200; i++)
            ability.Tick();
        Assert.True(ability.IsReady);
    }
}
