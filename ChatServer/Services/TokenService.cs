using System.Collections.Concurrent;

namespace ChatServer.Services
{
    public class TokenService
    {
        private readonly ConcurrentDictionary<string, string> _tokens = new();

        public string Issue(string userId)
        {
            var token = Guid.NewGuid().ToString();
            _tokens[token] = userId;
            return token;
        }

        public bool Validate(string token, out string? userId)
        {
            return _tokens.TryGetValue(token, out userId);
        }

        public void Revoke(string token)
        {
            _tokens.TryRemove(token, out _);
        }
    }
}