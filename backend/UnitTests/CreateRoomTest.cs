using conquerio.Game;

namespace UnitTests;

public class CreateRoomTest
{
    [Fact]
    public void GeneratesUniqueRoomIds()
    {
        var manager = new GameRoomManager();

        var r1 = manager.CreateRoom(null);
        var r2 = manager.CreateRoom(null);
        var r3 = manager.CreateRoom(null);

        var ids = new HashSet<string> { r1.RoomId, r2.RoomId, r3.RoomId };
        Assert.Equal(3, ids.Count);
    }

    [Fact]
    public void GeneratesUniqueNames()
    {
        var manager = new GameRoomManager();

        var r1 = manager.CreateRoom(null);
        var r2 = manager.CreateRoom(null);

        Assert.NotEqual(r1.Name, r2.Name);
    }

    [Fact]
    public void UsesCustomNameWhenProvided()
    {
        var manager = new GameRoomManager();

        var room = manager.CreateRoom("My Room");

        Assert.Equal("My Room", room.Name);
    }
}
