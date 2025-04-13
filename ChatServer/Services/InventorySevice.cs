using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class InventorySevice
    {
        private readonly ConcurrentDictionary<WebSocket, List<string>> _inventory = new();

        public void AddItem(WebSocket client, string item)
        {
            if (_inventory.TryGetValue(client, out List<string>? items))
            {
                items.Add(item);
            } else
            {
                _inventory[client] = [item];
            }
        }

        public List<string> GetItems(WebSocket client)
        {
            if (_inventory.TryGetValue(client, out List<string>? items))
            {
                return items;
            }
            return [];
        }
    }
}
