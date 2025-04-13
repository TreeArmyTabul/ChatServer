using ChatServer.Utils;
using ChatServer.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace ChatServer.Services
{
    public class UserRepository
    {
        private readonly string _filePath = Path.Combine("Data", "users.json");
        private readonly ConcurrentDictionary<string, UserRecord> _users;

        public UserRepository()
        {
            Directory.CreateDirectory("Data");

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _users = JsonSerializer.Deserialize<ConcurrentDictionary<string, UserRecord>>(json) ?? [];
            } else
            {
                _users = [];
            }
        }

        public string? GetNickname(string id)
        {
            return _users.TryGetValue(id, out var user) ? user.Nickname : null;
        }

        private string Hash(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(SHA256.HashData(bytes));
        }

        public bool Login(string id, string password)
        {
            if (!_users.TryGetValue(id, out var user))
            {
                return false;
            }
            return user.PasswordHash == Hash(password);
        }

        public bool Register(string id, string password)
        {
            if (_users.ContainsKey(id))
            {
                return false;
            }
            _users[id] = new UserRecord(id, NicknameGenerator.Generate(), Hash(password));
            Save();
            return true;
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_users);
            File.WriteAllText(_filePath, json);
        }
    }
}
