namespace conquerio.Game;

public static class TerritoryResolver
{
    public static List<(int X, int Y)> Resolve(byte[,] grid, PlayerState player)
    {
        var trail = player.Trail;
        if (trail.Count == 0)
            return new List<(int X, int Y)>();

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        byte colorId = player.ColorId;

        // create working grid with trail marked as player territory
        var working = new byte[width, height];
        Buffer.BlockCopy(grid, 0, working, 0, grid.Length);
        foreach (var (x, y) in trail)
        {
            working[x, y] = colorId;
        }

        // flood fill from edges to find all cells reachable from outside
        var reachable = new bool[width, height];
        var queue = new Queue<(int X, int Y)>();

        // seed with edge cells that are not player territory
        for (int x = 0; x < width; x++)
        {
            if (working[x, 0] != colorId && !reachable[x, 0])
            {
                reachable[x, 0] = true;
                queue.Enqueue((x, 0));
            }
            if (working[x, height - 1] != colorId && !reachable[x, height - 1])
            {
                reachable[x, height - 1] = true;
                queue.Enqueue((x, height - 1));
            }
        }
        for (int y = 1; y < height - 1; y++)
        {
            if (working[0, y] != colorId && !reachable[0, y])
            {
                reachable[0, y] = true;
                queue.Enqueue((0, y));
            }
            if (working[width - 1, y] != colorId && !reachable[width - 1, y])
            {
                reachable[width - 1, y] = true;
                queue.Enqueue((width - 1, y));
            }
        }

        // bfs flood fill
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height &&
                    !reachable[nx, ny] && working[nx, ny] != colorId)
                {
                    reachable[nx, ny] = true;
                    queue.Enqueue((nx, ny));
                }
            }
        }

        // collect enclosed cells (not reachable, not already player territory in original grid)
        // plus all trail cells
        var trailSet = new HashSet<(int, int)>(trail);
        var claimed = new List<(int X, int Y)>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (trailSet.Contains((x, y)))
                {
                    claimed.Add((x, y));
                }
                else if (!reachable[x, y] && grid[x, y] != colorId)
                {
                    claimed.Add((x, y));
                }
            }
        }

        return claimed;
    }

    public static bool IsOnOwnTerritory(byte[,] grid, int x, int y, byte colorId)
    {
        return grid[x, y] == colorId;
    }
}
