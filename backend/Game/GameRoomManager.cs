using System.Collections.Concurrent;

namespace conquerio.Game;

public class GameRoomManager
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly ConcurrentDictionary<string, DateTime> _emptyRoomTimestamps = new();
    private int _roomCounter;

    private static readonly TimeSpan EmptyRoomTimeout = TimeSpan.FromSeconds(60);

    public GameRoom GetOrCreateRoom()
    {
        // pick the non-empty room with the most players that isn't full
        GameRoom? best = null;
        foreach (var room in _rooms.Values)
        {
            if (room.IsFull || room.Players.IsEmpty) continue;
            if (best == null || room.Players.Count > best.Players.Count)
                best = room;
        }

        return best ?? CreateRoom(null);
    }

    public GameRoom CreateRoom(string? name)
    {
        var counter = Interlocked.Increment(ref _roomCounter);
        var id = $"room-{counter}";
        var roomName = name ?? $"Room {counter}";
        var newRoom = new GameRoom(id, roomName);
        _rooms[id] = newRoom;
        // start cleanup timer - gets cancelled when someone joins
        MarkEmpty(id);
        return newRoom;
    }

    public GameRoom? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public IEnumerable<GameRoom> GetAllRooms() => _rooms.Values;

    public void RemoveRoom(string roomId)
    {
        _rooms.TryRemove(roomId, out _);
        _emptyRoomTimestamps.TryRemove(roomId, out _);
    }

    public void MarkEmpty(string roomId)
    {
        _emptyRoomTimestamps[roomId] = DateTime.UtcNow;
    }

    public void CancelEmpty(string roomId)
    {
        _emptyRoomTimestamps.TryRemove(roomId, out _);
    }

    public void CleanupEmptyRooms()
    {
        var now = DateTime.UtcNow;
        foreach (var (roomId, emptySince) in _emptyRoomTimestamps)
        {
            if (now - emptySince < EmptyRoomTimeout) continue;

            // double check it's still empty before removing
            if (_rooms.TryGetValue(roomId, out var room) && room.Players.IsEmpty)
            {
                RemoveRoom(roomId);
            }
            else
            {
                // room got players again, cancel the timer
                _emptyRoomTimestamps.TryRemove(roomId, out _);
            }
        }
    }

    public GameRoom? FindRoomForPlayer(string playerId)
    {
        foreach (var room in _rooms.Values)
        {
            if (room.Players.ContainsKey(playerId)) return room;
        }
        return null;
    }
}
