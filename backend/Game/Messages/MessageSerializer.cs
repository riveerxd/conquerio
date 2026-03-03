using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace conquerio.Game.Messages;

public static class MessageSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static ClientMessage? Deserialize(byte[] buffer, int count)
    {
        try
        {
            return JsonSerializer.Deserialize<ClientMessage>(
                new ReadOnlySpan<byte>(buffer, 0, count), Options);
        }
        catch
        {
            return null;
        }
    }

    public static async Task SendAsync<T>(WebSocket socket, T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(message, Options);
        await socket.SendAsync(
            new ArraySegment<byte>(json),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: ct);
    }
}
