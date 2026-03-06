using conquerio.Game;

namespace UnitTests;

public class FindRoomForPlayerTest
{
    [Fact]
    public void FindsCorrectRoom()
    {
        var manager = new GameRoomManager();
        var r1 = manager.CreateRoom(null);
        var r2 = manager.CreateRoom(null);

        r1.AddPlayer("p1", "Alice", null!);
        r2.AddPlayer("p2", "Bob", null!);

        var result = manager.FindRoomForPlayer("p2");

        Assert.NotNull(result);
        Assert.Equal(r2.RoomId, result.RoomId);
    }

    [Fact]
    public void ReturnsNullWhenPlayerNotInAnyRoom()
    {
        var manager = new GameRoomManager();
        manager.CreateRoom(null);

        var result = manager.FindRoomForPlayer("unknown");

        Assert.Null(result);
    }
}
