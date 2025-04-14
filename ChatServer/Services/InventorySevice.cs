using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class InventorySevice
    {
        private readonly ConcurrentDictionary<string, List<string>> _inventory = new();

        public void AddItem(string userId, string item)
        {
            if (_inventory.TryGetValue(userId, out List<string>? items))
            {
                items.Add(item);
            } else
            {
                _inventory[userId] = [item];
            }
        }

        public List<string> GetItems(string userId)
        {
            if (_inventory.TryGetValue(userId, out List<string>? items))
            {
                return items;
            }
            return [];
        }
    }
}
