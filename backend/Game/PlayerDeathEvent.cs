namespace conquerio.Game;

public record PlayerDeathEvent(
    string VictimId,
    string? KillerId,
    string DeathCause,
    int Kills,
    float MaxTerritoryPct,
    DateTime StartedAt
);

