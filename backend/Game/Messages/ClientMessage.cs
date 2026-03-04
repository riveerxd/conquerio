using System.Text.Json.Serialization;

namespace conquerio.Game.Messages;

public class ClientMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("dir")]
    public string? Dir { get; set; }

    [JsonPropertyName("t")]
    public long? T { get; set; }

    [JsonPropertyName("ability")]
    public string? Ability { get; set; }
}
