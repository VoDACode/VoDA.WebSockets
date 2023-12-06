using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace VoDA.WebSockets.Services
{
    internal class ClientManager
    {
        private Dictionary<string, WebSocketClient> clients { get; } = new Dictionary<string, WebSocketClient>();

        public static ClientManager Instance { get; } = new ClientManager();
        private ClientManager()
        {
        }

        public WebSocketClient Add(HttpContext context, WebSocket socket)
        {
            var id = Guid.NewGuid().ToString() + DateTime.UtcNow.Ticks.ToString();
            var client = new WebSocketClient(context, socket, id);
            clients.Add(id, client);
            return client;
        }

        public async Task Remove(WebSocketClient client)
        {
            if (client is null)
                return;

            if (client.Socket.State > WebSocketState.Open)
            {
                try
                {
                    await client.CloseAsync();
                }
                catch (Exception ex)
                {
                }
            }
            clients.Remove(client.Id);
        }

        public void Remove(string id)
        {
            clients.Remove(id);
        }

        public async Task Clear()
        {
            foreach (var client in clients)
            {
                await client.Value.CloseAsync();
            }
        }

        public async Task Broadcast(string message)
        {
            foreach (var client in clients)
            {
                await client.Value.SendAsync(message);
            }
        }

        public async Task Broadcast(byte[] bytes)
        {
            foreach (var client in clients)
            {
                await client.Value.SendAsync(bytes);
            }
        }
    }
}
