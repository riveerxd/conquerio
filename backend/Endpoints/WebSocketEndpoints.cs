using System.Net.WebSockets;
using System.Text;

namespace conquerio.Endpoints;

public static class WebSocketEndpoints
{
    public static void MapWebSocketEndpoints(this WebApplication app)
    {
        // WS /ws/game - Real-time game communication
        app.Map("/ws/game", async (HttpContext context) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Expected a WebSocket request.");
                return;
            }

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[1024 * 4];

            // Send a welcome message
            var welcome = Encoding.UTF8.GetBytes("Not implemented yet");
            await ws.SendAsync(
                new ArraySegment<byte>(welcome),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: context.RequestAborted);

            // Keep connection open until client closes
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
                if (result.MessageType == WebSocketMessageType.Close)
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", context.RequestAborted);
            }
        });
    }
}

