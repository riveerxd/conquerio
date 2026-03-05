using System.Diagnostics;

namespace conquerio.Game;

public class GameTickHostedService : BackgroundService
{
    private readonly GameRoomManager _roomManager;
    private readonly ILogger<GameTickHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(50); // 20 ticks/sec

    public GameTickHostedService(GameRoomManager roomManager, ILogger<GameTickHostedService> logger)
    {
        _roomManager = roomManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Game tick loop started at {TickRate} ticks/sec", 20);

        while (!ct.IsCancellationRequested)
        {
            var start = Stopwatch.GetTimestamp();

            foreach (var room in _roomManager.GetAllRooms())
            {
                try
                {
                    room.Tick();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ticking room {RoomId}", room.RoomId);
                }
            }

            _roomManager.CleanupEmptyRooms();

            var elapsed = Stopwatch.GetElapsedTime(start);
            var delay = _interval - elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
        }
    }
}
