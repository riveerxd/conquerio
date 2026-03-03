using System.Collections.Concurrent;

namespace conquerio.Game;

public class GameRoomManager
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private int _roomCounter;

    public GameRoom GetOrCreateRoom()
    {
        // find a room that isn't full
        foreach (var room in _rooms.Values)
        {
            if (!room.IsFull) return room;
        }

        // create new room
        var id = $"room-{Interlocked.Increment(ref _roomCounter)}";
        var newRoom = new GameRoom(id);
        _rooms[id] = newRoom;
        return newRoom;
    }

    public IEnumerable<GameRoom> GetAllRooms() => _rooms.Values;

    public void RemoveRoom(string roomId) => _rooms.TryRemove(roomId, out _);

    public GameRoom? FindRoomForPlayer(string playerId)
    {
        foreach (var room in _rooms.Values)
        {
            if (room.Players.ContainsKey(playerId)) return room;
        }
        return null;
    }
}
