namespace conquerio.Game;

public record RoomSettings
{
    public int GridWidth { get; init; } = 200;
    public int GridHeight { get; init; } = 200;
    public int MaxPlayers { get; init; } = 20;
    public bool AbilitiesEnabled { get; init; } = true;
    public string? JoinCode { get; init; } = null;
}
