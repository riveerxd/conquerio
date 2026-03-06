using conquerio.Game;

namespace UnitTests;

public class RemovePlayerTest
{
    [Fact]
    public void RemovesPlayerFromRoom()
    {
        var room = new GameRoom("test-room", "Test");
        room.AddPlayer("p1", "Alice", null!);

        room.RemovePlayer("p1");

        Assert.Empty(room.Players);
    }
}
