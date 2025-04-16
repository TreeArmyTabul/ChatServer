using System.Collections.Concurrent;
using System.Text.Json;

namespace ChatServer.Services
{
    public class InventoryRepository
    {
        private readonly string _filePath = Path.Combine("Data", "inventories.json");
        private readonly ConcurrentDictionary<string, List<string>> _store;

        public InventoryRepository()
        {
            Directory.CreateDirectory("Data");

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _store = JsonSerializer.Deserialize<ConcurrentDictionary<string, List<string>>>(json) ?? [];
            }
            else
            {
                _store = [];
            }
        }

        public void AddItem(string userId, string item)
        {
            if (_store.TryGetValue(userId, out List<string>? items))
            {
                items.Add(item);
            } else
            {
                _store[userId] = [item];
            }
            Save();
        }

        public List<string> GetItems(string userId)
        {
            if (_store.TryGetValue(userId, out List<string>? items))
            {
                return items;
            }
            return [];
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_store);
            File.WriteAllText(_filePath, json);
        }
    }
}
