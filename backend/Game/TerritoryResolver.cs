namespace conquerio.Game;

public static class TerritoryResolver
{
    public static List<(int X, int Y)> Resolve(byte[,] grid, PlayerState player)
    {
        // TODO: flood-fill to claim enclosed territory when player
        // returns to their own territory with an active trail
        return new List<(int X, int Y)>();
    }
}
