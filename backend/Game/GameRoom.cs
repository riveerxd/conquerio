using System.Collections.Concurrent;
using conquerio.Game.Messages;
using Serilog;

namespace conquerio.Game;

public class GameRoom
{
    public string RoomId { get; }
    public string Name { get; }
    public int GridWidth { get; } = 200;
    public int GridHeight { get; } = 200;
    public int TickRate { get; } = 20;
    public int MaxPlayers { get; } = 20;
    private int TotalCells => GridWidth * GridHeight;

    public int BoostLengthSeconds { get; } = 3;
    public int BoostCooldownLengthSeconds { get; } = 10;

    public byte[,] Grid { get; }
    public ConcurrentDictionary<string, PlayerState> Players { get; } = new();
    public ConcurrentQueue<PlayerInput> InputQueue { get; } = new();

    /// <summary>Fired on the tick thread whenever a player is killed.</summary>
    public event Action<PlayerDeathEvent>? PlayerDied;

    private long _tick;
    private byte _nextColorId = 1;
    private readonly List<GridCell> _gridDiff = new();
    private readonly Random _rng = new();

    public GameRoom(string roomId, string name)
    {
        RoomId = roomId;
        Name = name;
        Grid = new byte[GridWidth, GridHeight];
    }

    public bool IsFull => Players.Count >= MaxPlayers;

    public PlayerState AddPlayer(string playerId, string username, System.Net.WebSockets.WebSocket socket)
    {
        var colorId = _nextColorId++;
        var spawnX = _rng.Next(20, GridWidth - 20);
        var spawnY = _rng.Next(20, GridHeight - 20);

        // claim 3x3 spawn territory (spawn coords are >=20 from edges, so all 9 cells fit)
        int spawnCells = 0;
        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                int tx = spawnX + ox;
                int ty = spawnY + oy;
                if (tx >= 0 && tx < GridWidth && ty >= 0 && ty < GridHeight)
                {
                    Grid[tx, ty] = colorId;
                    _gridDiff.Add(new GridCell { X = tx, Y = ty, C = colorId });
                    spawnCells++;
                }
            }
        }

        var player = new PlayerState
        {
            PlayerId = playerId,
            Username = username,
            X = spawnX,
            Y = spawnY,
            ColorId = colorId,
            Socket = socket,
            OwnedCells = spawnCells
        };

        Players[playerId] = player;

        Log.Information("Player {PlayerId} joined room {RoomId}. Current players: {PlayerCount}. Metric: PlayersPerRoom",
            playerId, RoomId, Players.Count);

        return player;
    }

    public void RemovePlayer(string playerId)
    {
        if (Players.TryRemove(playerId, out _))
        {
            Log.Information("Player {PlayerId} left room {RoomId}. Remaining players: {PlayerCount}. Metric: PlayersPerRoom",
                playerId, RoomId, Players.Count);
        }
    }

    public bool TryKillPlayer(string playerId, string? killerId, string cause)
    {
        if (!Players.TryGetValue(playerId, out var player))
            return false;

        return KillPlayer(player, killerId, cause);
    }

    public void Tick()
    {
        _tick++;
        _gridDiff.Clear();

        while (InputQueue.TryDequeue(out var input))
        {
            if (Players.TryGetValue(input.PlayerId, out var player) && player.IsAlive)
            {
                if (input.Direction != null && !IsOpposite(player.Direction, (Direction)input.Direction))
                    player.Direction = (Direction)input.Direction;

                if (input.Ability != null)
                {
                    switch (input.Ability)
                    {
                        case PlayerAbility.BOOST:
                            if (player.BoostCooldownTicksRemaining <= 0)
                                player.BoostTicksRemaining = TickRate * BoostLengthSeconds;
                            break;
                    }
                }
            }
        }

        // Collisions are checked against pre-movement positions for fairness in simultaneous edge cases.
        foreach (var p in Players.Values)
        {
            if (!p.IsAlive) continue;

            var (dx, dy) = GetDelta(p.Direction);

            // Handle boost ticks
            p.BoostCooldownTicksRemaining = Math.Max(0, p.BoostCooldownTicksRemaining - 1);
            if (p.BoostTicksRemaining > 0)
            {
                if (--p.BoostTicksRemaining <= 0)
                {
                    p.SpeedMultiplier = 1;
                    p.BoostCooldownTicksRemaining = BoostCooldownLengthSeconds * TickRate;
                }
                else p.SpeedMultiplier = 2;
            }

            int newX = p.X + dx * (int)p.SpeedMultiplier;
            int newY = p.Y + dy * (int)p.SpeedMultiplier;

            // clamp to grid bounds
            newX = Math.Clamp(newX, 0, GridWidth - 1);
            newY = Math.Clamp(newY, 0, GridHeight - 1);

            bool isOnTerritory = TerritoryResolver.IsOnOwnTerritory(Grid, newX, newY, p.ColorId);
            var traveledSpaces = new LinkedList<(int, int)>();

            // account for all the grid spaces the player traveled through
            var rangeX = newX > p.X ? (p.X + 1, newX) : (newX, p.X - 1);
            var rangeY = newY > p.Y ? (p.Y + 1, newY) : (newY, p.Y - 1);
            if (p.X != newX)
                for (int i = rangeX.Item1; i <= rangeX.Item2; i++)
                {
                    traveledSpaces.AddLast((i, newY));
                }

            if (p.Y != newY)
                for (int i = rangeY.Item1; i <= rangeY.Item2; i++)
                {
                    traveledSpaces.AddLast((newX, i));
                }

            p.X = newX;
            p.Y = newY;

            // --- collision: self trail ---
            if (CollisionDetector.HitsSelfTrail(newX, newY, p))
            {
                KillPlayer(p, killerId: null, "self");
                continue;
            }

            // --- collision: another player's trail ---
            var killer = Players.Values.FirstOrDefault(other =>
                other.PlayerId != p.PlayerId &&
                other.IsAlive &&
                CollisionDetector.HitsTrail(newX, newY, other));

            if (killer != null)
            {
                killer.Kills++;
                KillPlayer(p, killer.PlayerId, "trail");
                continue;
            }

            // --- collision: another player does not give kill credit ---
            if (CollisionDetector.HitsPlayer(newX, newY, Players.Values, p.PlayerId))
            {
                var other = Players.Values.FirstOrDefault(o => o.PlayerId != p.PlayerId && o.X == newX && o.Y == newY);
                if (other != null)
                {
                    KillPlayer(p, other.PlayerId, "Head-on");
                    continue;
                }
            }

            if (isOnTerritory && p.Trail.Count > 0)
            {
                // returned to own territory with trail - claim enclosed area
                ClaimTerritory(p);
            }
            else if (!isOnTerritory)
            {
                // outside territory - track trail
                if (p.Trail.Count == 0 || p.Trail[^1] != (newX, newY))
                {
                    p.Trail.Add((newX, newY));
                    foreach (var space in traveledSpaces)
                    {
                        bool isOnOwnTerritory =
                            TerritoryResolver.IsOnOwnTerritory(Grid, space.Item1, space.Item2, p.ColorId);
                        if (isOnOwnTerritory && p.Trail.Count > 0)
                        {
                            // returned to own territory with trail - claim enclosed area
                            ClaimTerritory(p);
                        }
                        else if (!isOnOwnTerritory)
                        {
                            // outside territory - track trail
                            if (p.Trail.Count == 0 || p.Trail[^1] != (space.Item1, space.Item2))
                            {
                                p.Trail.Add(space);
                            }
                        }
                    }
                }

                BroadcastState();
            }
        }

        // Check for players occupying the same position after movement (e.g., head-on collisions)
        var positionCounts = new Dictionary<(int, int), List<PlayerState>>();
        foreach (var p in Players.Values)
        {
            if (p.IsAlive)
            {
                var pos = (p.X, p.Y);
                if (!positionCounts.ContainsKey(pos)) positionCounts[pos] = new List<PlayerState>();
                positionCounts[pos].Add(p);
            }
        }
        foreach (var group in positionCounts.Values.Where(g => g.Count > 1))
        {
            foreach (var p in group)
            {
                KillPlayer(p, null, "collision");
            }
        }
    }

    private void BroadcastState()
    {
        var players = Players.Values
            .Where(ps => ps.IsAlive)
            .Select(ps => new PlayerDto
            {
                Id = ps.PlayerId,
                Username = ps.Username,
                X = ps.X,
                Y = ps.Y,
                Dir = ps.Direction.ToString().ToLower(),
                Trail = ps.Trail.Select(t => new[] { t.X, t.Y }).ToList(),
                Alive = ps.IsAlive,
                ColorId = ps.ColorId,
                SpeedMultiplier = ps.SpeedMultiplier
            })
            .ToList();

        var msg = new StateMessage
        {
            Tick = _tick,
            Players = players,
            GridDiff = _gridDiff.ToList()
        };

        foreach (var player in Players.Values)
        {
            if (player.Socket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                _ = MessageSerializer.SendAsync(player.Socket, msg);
            }
        }
    }

    public byte[] GetFlatGrid()
    {
        var flat = new byte[GridWidth * GridHeight];
        for (var y = 0; y < GridHeight; y++)
        for (var x = 0; x < GridWidth; x++)
            flat[y * GridWidth + x] = Grid[x, y];
        return flat;
    }

    private static (int dx, int dy) GetDelta(Direction dir) => dir switch
    {
        Direction.Up => (0, -1),
        Direction.Down => (0, 1),
        Direction.Left => (-1, 0),
        Direction.Right => (1, 0),
        _ => (0, 0)
    };

    private static bool IsOpposite(Direction a, Direction b) =>
        (a == Direction.Up && b == Direction.Down) ||
        (a == Direction.Down && b == Direction.Up) ||
        (a == Direction.Left && b == Direction.Right) ||
        (a == Direction.Right && b == Direction.Left);

    private bool KillPlayer(PlayerState player, string? killerId, string cause)
    {
        lock (player)
        {
            if (!player.IsAlive)
                return false;

            player.IsAlive = false;
            player.Trail.Clear();
        }

        var duration = DateTime.UtcNow - player.StartedAt;
        Log.Information("Player {PlayerId} died in room {RoomId} after {DurationSeconds}s. Cause: {Cause}. Metric: GameDuration",
            player.PlayerId, RoomId, duration.TotalSeconds, cause);

        Log.Information("Death in room {RoomId}. Victim: {PlayerId}, Killer: {KillerId}, Cause: {Cause}. Metric: Death",
            RoomId, player.PlayerId, killerId ?? "N/A", cause);

        var evt = new PlayerDeathEvent(
            victimId: player.PlayerId,
            killerId: killerId,
            deathCause: cause,
            kills: player.Kills,
            maxTerritoryPct: player.MaxTerritoryPct,
            startedAtUtc: player.StartedAt
        );

        var handlers = PlayerDied;
        if (handlers is null)
            return true;

        foreach (var d in handlers.GetInvocationList())
        {
            try
            {
                ((Action<PlayerDeathEvent>)d)(evt);
            }
            catch
            {
                // Event handlers are best-effort and must not crash the game loop.
            }
        }

        return true;
    }

    private void ClaimTerritory(PlayerState player)
    {
        var cellsToClaim = TerritoryResolver.Resolve(Grid, player);
        var ownersByColor = Players.Values
            .Where(ps => ps.IsAlive)
            .ToDictionary(ps => ps.ColorId, ps => ps);

        int newlyOwned = 0;
        foreach (var (x, y) in cellsToClaim)
        {
            var previousOwnerColor = Grid[x, y];
            if (previousOwnerColor == player.ColorId)
                continue;

            if (previousOwnerColor != 0 && ownersByColor.TryGetValue(previousOwnerColor, out var victim) &&
                victim.OwnedCells > 0)
                victim.OwnedCells--;

            Grid[x, y] = player.ColorId;
            _gridDiff.Add(new GridCell { X = x, Y = y, C = player.ColorId });
            newlyOwned++;
        }

        player.Trail.Clear();

        Log.Information("Player {PlayerId} claimed {CellCount} cells in room {RoomId}. Metric: TerritoryClaimed",
            player.PlayerId, newlyOwned, RoomId);

        // update best territory percentage incrementally
        player.OwnedCells += newlyOwned;
        var pct = player.OwnedCells * 100f / TotalCells;
        if (pct > player.MaxTerritoryPct)
            player.MaxTerritoryPct = pct;
    }
}
