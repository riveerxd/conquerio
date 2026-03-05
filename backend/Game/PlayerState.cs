using System.Net.WebSockets;
using conquerio.Game.Abilities;

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

    public LinkedList<PlayerAbility> Abilities { get; set; } = new();


    // changed speed multiplier to int because current grid logic utilizes integer coordinates
    public int SpeedMultiplier { get; set; } = 1;
}
