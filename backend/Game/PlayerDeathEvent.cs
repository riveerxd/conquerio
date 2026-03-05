namespace conquerio.Game;

public record PlayerDeathEvent
{
    public string VictimId { get; }
    public string? KillerId { get; }
    public string DeathCause { get; }
    public int Kills { get; }
    public float MaxTerritoryPct { get; }
    public DateTime StartedAtUtc { get; }

    public PlayerDeathEvent(
        string victimId,
        string? killerId,
        string deathCause,
        int kills,
        float maxTerritoryPct,
        DateTime startedAtUtc)
    {
        VictimId = victimId;
        KillerId = killerId;
        DeathCause = deathCause;
        Kills = kills;
        MaxTerritoryPct = maxTerritoryPct;
        StartedAtUtc = startedAtUtc.Kind == DateTimeKind.Utc
            ? startedAtUtc
            : startedAtUtc.ToUniversalTime();
    }
}

