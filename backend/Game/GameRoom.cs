using System.Collections.Concurrent;
using conquerio.Game.Messages;

namespace conquerio.Game;

public class GameRoom
{
    public string RoomId { get; }
    public int GridWidth { get; } = 200;
    public int GridHeight { get; } = 200;
    public int TickRate { get; } = 20;
    public int MaxPlayers { get; } = 20;

    public byte[,] Grid { get; }
    public ConcurrentDictionary<string, PlayerState> Players { get; } = new();
    public ConcurrentQueue<PlayerInput> InputQueue { get; } = new();

    private long _tick;
    private byte _nextColorId = 1;
    private readonly List<GridCell> _gridDiff = new();
    private readonly Random _rng = new();

    public GameRoom(string roomId)
    {
        RoomId = roomId;
        Grid = new byte[GridWidth, GridHeight];
    }

    public bool IsFull => Players.Count >= MaxPlayers;

    public PlayerState AddPlayer(string playerId, string username, System.Net.WebSockets.WebSocket socket)
    {
        var colorId = _nextColorId++;
        var spawnX = _rng.Next(20, GridWidth - 20);
        var spawnY = _rng.Next(20, GridHeight - 20);

        var player = new PlayerState
        {
            PlayerId = playerId,
            Username = username,
            X = spawnX,
            Y = spawnY,
            ColorId = colorId,
            Socket = socket
        };

        Players[playerId] = player;
        return player;
    }

    public void RemovePlayer(string playerId)
    {
        Players.TryRemove(playerId, out _);
    }

    public void Tick()
    {
        _tick++;
        _gridDiff.Clear();

        while (InputQueue.TryDequeue(out var input))
        {
            if (Players.TryGetValue(input.PlayerId, out var player) && player.IsAlive)
            {
                if (!IsOpposite(player.Direction, input.Direction))
                    player.Direction = input.Direction;
            }
        }

        foreach (var p in Players.Values)
        {
            if (!p.IsAlive) continue;

            var (dx, dy) = GetDelta(p.Direction);
            p.X += dx;
            p.Y += dy;

            // clamp to grid bounds
            if (p.X < 0) p.X = 0;
            if (p.X >= GridWidth) p.X = GridWidth - 1;
            if (p.Y < 0) p.Y = 0;
            if (p.Y >= GridHeight) p.Y = GridHeight - 1;
        }

        BroadcastState();
    }

    private void BroadcastState()
    {
        var players = Players.Values
            .Where(p => p.IsAlive)
            .Select(p => new PlayerDto
            {
                Id = p.PlayerId,
                X = p.X,
                Y = p.Y,
                Dir = p.Direction.ToString().ToLower(),
                Trail = p.Trail.Select(t => new[] { t.X, t.Y }).ToList(),
                Alive = p.IsAlive,
                ColorId = p.ColorId
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
}
