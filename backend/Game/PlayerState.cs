using System.Net.WebSockets;

namespace conquerio.Game;

public class PlayerState
{
    public required string PlayerId { get; set; }
    public required string Username { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Direction { get; set; } = Direction.Right;
    public bool IsAlive { get; set; } = true;
    public byte ColorId { get; set; }
    public List<(int X, int Y)> Trail { get; set; } = new();
    public required WebSocket Socket { get; set; }

    // future: abilities
    // changed speed multiplier to int because current grid logic utilizes integer coordinates
    public int SpeedMultiplier { get; set; } = 1;
}
