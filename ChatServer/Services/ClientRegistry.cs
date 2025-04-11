using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class ClientRegistry
    {
        private readonly ConcurrentDictionary<WebSocket, string> _clients = new();

        public bool TryAdd(WebSocket client, string nickname)
        {
            return _clients.TryAdd(client, nickname);
        }

        public string TryGetNickname(WebSocket client)
        {
            return _clients.TryGetValue(client, out var nickname) ? nickname : string.Empty;
        }

        public bool TryRemove(WebSocket client, out string nickname)
        {
            return _clients.TryRemove(client, out nickname);
        }

        public IEnumerable<KeyValuePair<WebSocket, string>> GetOtherClients(WebSocket sender)
        {
            return _clients.Where(kv => kv.Key != sender && kv.Key.State == WebSocketState.Open);
        }

        public int Count => _clients.Count;
    }
}
