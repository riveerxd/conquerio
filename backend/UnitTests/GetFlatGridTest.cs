using conquerio.Game;

namespace UnitTests;

public class GetFlatGridTest
{
    [Fact]
    public void ReturnsCorrectFlattenedGridArray()
    {
        var room = new GameRoom("test-room", "Test");
        room.Grid[0, 0] = 1;
        room.Grid[1, 0] = 2;
        room.Grid[0, 1] = 3;

        var flat = room.GetFlatGrid();

        Assert.Equal(room.GridWidth * room.GridHeight, flat.Length);
        Assert.Equal(1, flat[0]);
        Assert.Equal(2, flat[1]);
        Assert.Equal(3, flat[room.GridWidth]);
    }
}
