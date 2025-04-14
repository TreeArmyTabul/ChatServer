using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class ClientRegistry
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sessions = new();

        public async Task RegisterAsync(string userId, WebSocket socket)
        {
            if (_sessions.TryGetValue(userId, out var existingSession)) {
                if (existingSession.State == WebSocketState.Open) {
                    await existingSession.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "중복 접속으로 기존 연결을 종료합니다.", CancellationToken.None);
                }
            }
            _sessions[userId] = socket;
        }

        public bool TryRemove(string userId)
        {
            return _sessions.TryRemove(userId, out _);
        }

        public IEnumerable<KeyValuePair<string, WebSocket>> GetAllSockets()
        {
            return _sessions.Where(kv => kv.Value.State == WebSocketState.Open);
        }

        public WebSocket? GetSocketByUserId(string userId)
        {
            return _sessions.TryGetValue(userId, out var socket) ? socket : null;
        }

        public IEnumerable<KeyValuePair<string, WebSocket>> GetSocketsExcept(string userId)
        {
            return _sessions.Where(kv => kv.Key != userId && kv.Value.State == WebSocketState.Open);
        }
    }
}
