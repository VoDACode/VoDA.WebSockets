using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;

namespace VoDA.WebSockets
{
    public class WebSocketClient
    {
        public HttpContext Context { get; }
        public WebSocket Socket { get; }
        public string Id { get; }

        public WebSocketClient(HttpContext context, WebSocket socket, string id)
        {
            Context = context;
            Socket = socket;
            Id = id;
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendAsync(byte[] bytes)
        {
            await Socket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public async Task<string> ReceiveAsync(int bufferSize = 1024 * 4)
        {
            var buffer = new byte[bufferSize];
            var result = await Socket.ReceiveAsync(buffer, CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public async Task<byte[]> ReceiveBytesAsync(int bufferSize = 1024 * 4)
        {
            var buffer = new byte[bufferSize];
            var result = await Socket.ReceiveAsync(buffer, CancellationToken.None);
            return buffer.Take(result.Count).ToArray();
        }

        public async Task CloseAsync()
        {
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketManager", default);
        }

        public async Task CloseAsync(WebSocketCloseStatus status, string message)
        {
            await Socket.CloseAsync(status, message, default);
        }

        public async Task CloseAsync(WebSocketCloseStatus status, string message, CancellationToken cancellationToken)
        {
            await Socket.CloseAsync(status, message, cancellationToken);
        }
    }
}
