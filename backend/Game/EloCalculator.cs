namespace conquerio.Game;

public static class EloCalculator
{
    // Can tune later - stevek
    private const int KFactor = 32;

    public static (int victimNewElo, int? killerNewElo) Calculate(
        int victimOldElo,
        int? killerOldElo,
        float victimMaxTerritoryPct)
    {
        // Default Elo calculation for victim if no killer (e.g., suicide or environmental death)
        if (killerOldElo == null)
        {
            // Case 1: Suicide / Disconnect / Environmental death
            // We can model this by considering the victim lost to an average-skilled "game environment" (Elo 1000)
            const int averageElo = 1000;
            double expectedScore = GetExpectedScore(victimOldElo, averageElo);

            // Actual score is 0.0 (lost).
            // Modifying K based on territory percentage - if they had 90% territory, they should lose more Elo
            // because they lost more "value". Or simpler: use territory as a multiplier to the loss.
            double territoryMultiplier = 1.0 + (victimMaxTerritoryPct / 100.0);
            int diff = (int)Math.Round(KFactor * (0.0 - expectedScore) * territoryMultiplier);

            return (Math.Max(0, victimOldElo + diff), null);
        }

        // Case 2: Killed by another player
        double expectedVictimScore = GetExpectedScore(victimOldElo, killerOldElo.Value);
        double expectedKillerScore = 1.0 - expectedVictimScore;

        // Victim actual score = 0.0, Killer actual score = 1.0
        // Use territory percentage as a multiplier to spice things up.
        // A player with higher territory percentage yields more reward for the killer and deeper loss for the victim.
        double territoryMod = 1.0 + (victimMaxTerritoryPct / 50.0); // e.g. 50% territory doubles the change

        int victimDiff = (int)Math.Round(KFactor * (0.0 - expectedVictimScore) * territoryMod);
        int killerDiff = (int)Math.Round(KFactor * (1.0 - expectedKillerScore) * territoryMod);

        return (
            Math.Max(0, victimOldElo + victimDiff),
            Math.Max(0, killerOldElo.Value + killerDiff)
        );
    }

    private static double GetExpectedScore(int playerElo, int opponentElo)
    {
        return 1.0 / (1.0 + Math.Pow(10, (opponentElo - playerElo) / 400.0));
    }
}

