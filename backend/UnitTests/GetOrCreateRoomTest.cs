using conquerio.Game;

namespace UnitTests;

public class GetOrCreateRoomTest
{
    [Fact]
    public void ReturnsNonFullRoomWithMostPlayers()
    {
        var manager = new GameRoomManager();

        var r1 = manager.CreateRoom(null);
        r1.AddPlayer("p1", "A", null!);

        var r2 = manager.CreateRoom(null);
        r2.AddPlayer("p2", "B", null!);
        r2.AddPlayer("p3", "C", null!);
        r2.AddPlayer("p4", "D", null!);

        var r3 = manager.CreateRoom(null);
        r3.AddPlayer("p5", "E", null!);
        r3.AddPlayer("p6", "F", null!);

        var result = manager.GetOrCreateRoom();

        Assert.Equal(r2.RoomId, result.RoomId);
    }

    [Fact]
    public void CreatesNewRoomWhenAllRoomsAreFull()
    {
        var manager = new GameRoomManager();

        var r1 = manager.CreateRoom(null);
        for (int i = 0; i < 20; i++)
            r1.AddPlayer($"p{i}", $"P{i}", null!);

        var initialCount = manager.GetAllRooms().Count();
        var result = manager.GetOrCreateRoom();

        Assert.NotEqual(r1.RoomId, result.RoomId);
        Assert.Equal(initialCount + 1, manager.GetAllRooms().Count());
    }

    [Fact]
    public void CreatesNewRoomWhenAllRoomsAreEmpty()
    {
        var manager = new GameRoomManager();

        manager.CreateRoom(null);
        manager.CreateRoom(null);

        var result = manager.GetOrCreateRoom();

        Assert.Equal(3, manager.GetAllRooms().Count());
    }
}
