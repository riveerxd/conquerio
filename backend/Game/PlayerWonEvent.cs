namespace conquerio.Game;

public record PlayerWonEvent(
    string WinnerId,
    int Kills,
    float MaxTerritoryPct,
    DateTime StartedAtUtc);
