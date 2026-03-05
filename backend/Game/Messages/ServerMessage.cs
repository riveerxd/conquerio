using System.Text.Json.Serialization;

namespace conquerio.Game.Messages;

public class JoinedMessage
{
    [JsonPropertyName("type")]
    public string Type => "joined";

    public required string PlayerId { get; set; }
    public byte ColorId { get; set; }
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public int TickRate { get; set; }

    /// <summary>
    /// Run-length encoded grid data.
    /// Format: [count, value, count, value, ...]
    /// </summary>
    public required byte[] RleGrid { get; set; }
}

public class StateMessage
{
    [JsonPropertyName("type")]
    public string Type => "state";

    public long Tick { get; set; }
    public required List<PlayerDto> Players { get; set; }
    public required List<GridCell> GridDiff { get; set; }
}

public class PlayerDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public required string Dir { get; set; }
    public required List<int[]> Trail { get; set; }
    public bool Alive { get; set; }
    public bool Disconnected { get; set; }
    public byte ColorId { get; set; }
    public int SpeedMultiplier { get; set; }
    public IEnumerable<AbilityDto> Abilities { get; set; } = [];
}

public class GridCell
{
    public int X { get; set; }
    public int Y { get; set; }
    public byte C { get; set; }
}

public class AbilityDto
{
    public required string Name { get; set; }
    public float DurationSecondsRemaining { get; set; }
    public float CooldownSecondsRemaining { get; set; }
}

public class DeathMessage
{
    [JsonPropertyName("type")]
    public string Type => "death";

    public string? KilledBy { get; set; }
    public required string Reason { get; set; }
}

public class PongMessage
{
    [JsonPropertyName("type")]
    public string Type => "pong";

    public long T { get; set; }
}

public class ErrorMessage
{
    [JsonPropertyName("type")]
    public string Type => "error";

    public required string Msg { get; set; }
}
