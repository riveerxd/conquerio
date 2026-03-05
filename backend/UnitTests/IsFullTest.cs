using conquerio.Game;

namespace UnitTests;

public class IsFullTest
{
    [Fact]
    public void ReturnsTrueWhen20PlayersInRoom()
    {
        var room = new GameRoom("test-room", "Test");

        for (int i = 0; i < 20; i++)
            room.AddPlayer($"p{i}", $"Player{i}", null!);

        Assert.True(room.IsFull);
    }

    [Fact]
    public void ReturnsFalseWhenNotFull()
    {
        var room = new GameRoom("test-room", "Test");
        room.AddPlayer("p1", "Alice", null!);

        Assert.False(room.IsFull);
    }
}
