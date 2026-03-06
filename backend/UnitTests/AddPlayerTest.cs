using conquerio.Game;

namespace UnitTests;

public class AddPlayerTest
{
    private GameRoom CreateRoom() => new("test-room", "Test");

    [Fact]
    public void SpawnsPlayerWithinValidBounds()
    {
        var room = CreateRoom();

        var player = room.AddPlayer("p1", "Alice", null!);

        Assert.InRange(player.X, 20, room.GridWidth - 21);
        Assert.InRange(player.Y, 20, room.GridHeight - 21);
    }

    [Fact]
    public void Claims3x3TerritoryAroundSpawn()
    {
        var room = CreateRoom();

        var player = room.AddPlayer("p1", "Alice", null!);

        for (int ox = -1; ox <= 1; ox++)
        for (int oy = -1; oy <= 1; oy++)
            Assert.Equal(player.ColorId, room.Grid[player.X + ox, player.Y + oy]);

        Assert.Equal(9, player.OwnedCells);
    }

    [Fact]
    public void AssignsUniqueColorIds()
    {
        var room = CreateRoom();

        var p1 = room.AddPlayer("p1", "Alice", null!);
        var p2 = room.AddPlayer("p2", "Bob", null!);
        var p3 = room.AddPlayer("p3", "Charlie", null!);

        var colors = new HashSet<byte> { p1.ColorId, p2.ColorId, p3.ColorId };
        Assert.Equal(3, colors.Count);
    }
}
