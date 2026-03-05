namespace conquerio.Game;

public static class CollisionDetector
{
    public static bool IsOutOfBounds(int x, int y, int width, int height)
    {
        return x < 0 || x >= width || y < 0 || y >= height;
    }

    /// <summary>
    /// Returns true if (x, y) lies on the trail of the given player
    /// (used to detect whether one player runs into another player's trail).
    /// </summary>
    public static bool HitsTrail(int x, int y, PlayerState player)
    {
        foreach (var (tx, ty) in player.Trail)
            if (tx == x && ty == y) return true;
        return false;
    }

    /// <summary>
    /// Returns true if (x, y) lies on the trail of any player in the
    /// collection, optionally excluding one player by id.
    /// </summary>
    public static bool HitsTrail(int x, int y, IEnumerable<PlayerState> players, string excludePlayerId)
    {
        foreach (var player in players)
        {
            if (player.PlayerId == excludePlayerId) continue;
            if (HitsTrail(x, y, player)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if (x, y) lies somewhere on the player's own trail
    /// (i.e. the player has looped back into themselves).
    /// The very last point is the current head position, so we exclude it
    /// to avoid an immediate false-positive on the first step out of territory.
    /// </summary>
    public static bool HitsSelfTrail(int x, int y, PlayerState player)
    {
        var trail = player.Trail;
        int count = trail.Count;
        for (int i = 0; i < count - 1; i++)
            if (trail[i].X == x && trail[i].Y == y) return true;
        return false;
    }
}
