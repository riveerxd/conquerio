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
    public bool Invulnerable { get; set; } = false;
    public byte ColorId { get; set; }
    public List<(int X, int Y)> Trail { get; set; } = new();
    public required WebSocket Socket { get; set; }
    public bool IsDisconnected { get; set; }
    public long DisconnectedAtTick { get; set; }

    public List<PlayerAbility> Abilities { get; set; } = new();

    // stats tracked during a run
    public int Kills { get; set; }
    public int OwnedCells { get; set; }
    public float MaxTerritoryPct { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    // future: abilities
    public float SpeedMultiplier { get; set; } = 1.0f;
}
