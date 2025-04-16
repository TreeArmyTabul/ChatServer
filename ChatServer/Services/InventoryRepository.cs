using ChatServer.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ChatServer.Services
{
    public class InventoryRepository
    {
        private readonly string _filePath = Path.Combine("Data", "inventories.json");
        private readonly ConcurrentDictionary<string, List<Item>> _store;

        public InventoryRepository()
        {
            Directory.CreateDirectory("Data");

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _store = JsonSerializer.Deserialize<ConcurrentDictionary<string, List<Item>>>(json) ?? [];
            }
            else
            {
                _store = [];
            }
        }

        public void AddItem(string userId, Item item)
        {
            if (_store.TryGetValue(userId, out List<Item>? items))
            {
                items.Add(item);
            } else
            {
                _store[userId] = [item];
            }
            Save();
        }

        public List<Item> GetItems(string userId)
        {
            if (_store.TryGetValue(userId, out List<Item>? items))
            {
                return items;
            }
            return [];
        }

        public Dictionary<string, int> GetUserValues()
        {
            return _store.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Sum(item => item.Value)
            );
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_store);
            File.WriteAllText(_filePath, json);
        }
    }
}
