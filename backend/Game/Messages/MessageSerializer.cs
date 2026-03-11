using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;

namespace conquerio.Game.Messages;

public static class MessageSerializer
{
    private static long _messagesSent;
    private static long _messagesReceived;
    private static DateTime _lastReported = DateTime.UtcNow;
    private static readonly object _lock = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static ClientMessage? Deserialize(byte[] buffer, int count)
    {
        try
        {
            IncrementReceived();
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
        IncrementSent();
    }

    private static void IncrementSent()
    {
        Interlocked.Increment(ref _messagesSent);
        CheckReport();
    }

    private static void IncrementReceived()
    {
        Interlocked.Increment(ref _messagesReceived);
        CheckReport();
    }

    private static void CheckReport()
    {
        if ((DateTime.UtcNow - _lastReported).TotalSeconds >= 60)
        {
            lock (_lock)
            {
                if ((DateTime.UtcNow - _lastReported).TotalSeconds >= 60)
                {
                    var sent = Interlocked.Exchange(ref _messagesSent, 0);
                    var received = Interlocked.Exchange(ref _messagesReceived, 0);
                    var elapsed = DateTime.UtcNow - _lastReported;
                    _lastReported = DateTime.UtcNow;

                    Log.Information("WebSocket Throughput: Sent {SentCount}, Received {ReceivedCount} over {ElapsedSeconds}s. Metric: Throughput",
                        sent, received, elapsed.TotalSeconds);
                }
            }
        }
    }
}
