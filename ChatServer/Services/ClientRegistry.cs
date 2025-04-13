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

        public bool TryGetNickname(WebSocket client, out string nickname)
        {
            return _clients.TryGetValue(client, out nickname);
        }

        public bool TryRemove(WebSocket client, out string nickname)
        {
            return _clients.TryRemove(client, out nickname);
        }

        public IEnumerable<KeyValuePair<WebSocket, string>> GetAllClients()
        {
            return _clients.Where(kv => kv.Key.State == WebSocketState.Open);
        }

        public IEnumerable<KeyValuePair<WebSocket, string>> GetOtherClients(WebSocket sender)
        {
            return _clients.Where(kv => kv.Key != sender && kv.Key.State == WebSocketState.Open);
        }

        public WebSocket? GetRandomClient(WebSocket sender)
        {
            var others = GetOtherClients(sender).ToList();
   
            if (others.Count == 0)
            {
                return null;
            }

            var random = new Random();
            var selected = others[random.Next(others.Count)];
            return selected.Key;
        }


        public int Count => _clients.Count;
    }
}
