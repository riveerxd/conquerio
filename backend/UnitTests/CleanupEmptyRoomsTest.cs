using System.Collections.Concurrent;
using System.Reflection;
using conquerio.Game;

namespace UnitTests;

public class CleanupEmptyRoomsTest
{
    [Fact]
    public void RemovesRoomAfterTimeout()
    {
        var manager = new GameRoomManager();
        var room = manager.CreateRoom(null);

        manager.MarkEmpty(room.RoomId);
        BackdateTimestamp(manager, room.RoomId, TimeSpan.FromMinutes(5));
        manager.CleanupEmptyRooms();

        Assert.Null(manager.GetRoom(room.RoomId));
    }

    [Fact]
    public void KeepsRoomsThatGotPlayersBeforeTimeout()
    {
        var manager = new GameRoomManager();
        var room = manager.CreateRoom(null);

        manager.MarkEmpty(room.RoomId);
        room.AddPlayer("p1", "Alice", null!);
        BackdateTimestamp(manager, room.RoomId, TimeSpan.FromMinutes(5));
        manager.CleanupEmptyRooms();

        Assert.NotNull(manager.GetRoom(room.RoomId));
    }

    private static void BackdateTimestamp(GameRoomManager manager, string roomId, TimeSpan age)
    {
        var field = typeof(GameRoomManager)
            .GetField("_emptyRoomTimestamps", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = (ConcurrentDictionary<string, DateTime>)field.GetValue(manager)!;
        dict[roomId] = DateTime.UtcNow - age;
    }
}
