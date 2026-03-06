using conquerio.Game;

namespace UnitTests;

public class GetRoomTest
{
    [Fact]
    public void ReturnsCorrectRoom()
    {
        var manager = new GameRoomManager();
        var room = manager.CreateRoom(null);

        var result = manager.GetRoom(room.RoomId);

        Assert.NotNull(result);
        Assert.Equal(room.RoomId, result.RoomId);
    }

    [Fact]
    public void ReturnsNullForNonExistentRoom()
    {
        var manager = new GameRoomManager();

        var result = manager.GetRoom("nonexistent");

        Assert.Null(result);
    }
}
