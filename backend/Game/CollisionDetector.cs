namespace conquerio.Game;

public static class CollisionDetector
{
    public static bool IsOutOfBounds(int x, int y, int width, int height)
    {
        return x < 0 || x >= width || y < 0 || y >= height;
    }

    // TODO: iterate through all player trails and check for (x,y) match
    // might need to optimize this if it gets slow with many players
    public static bool HitsTrail(int x, int y, IEnumerable<PlayerState> players, string excludePlayerId)
    {
        return false;
    }

    public static bool HitsSelfTrail(int x, int y, PlayerState player)
    {
        // TODO
        return false;
    }
}
