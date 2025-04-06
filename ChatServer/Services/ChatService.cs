using ChatServer.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ChatServer.Services
{
    public class ChatService
    {
        private readonly List<WebSocket> _clients = new();

        public void AddClient(WebSocket socket)
        {
            _clients.Add(socket);
            Console.WriteLine("클라이언트 연결.");
        }
        public void RemoveClient(WebSocket socket)
        {
            _clients.Remove(socket);
            Console.WriteLine("클라이언트 연결 해제.");
        }

        public async Task BroadcastMessageAsync(string json, WebSocket sender)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(json);
                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                {
                    return;
                }

                var buffer = Encoding.UTF8.GetBytes(json);

                foreach (var otherClient in _clients)
                {
                    if (otherClient != sender && otherClient.State == WebSocketState.Open)
                    {
                        await otherClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

            } catch (Exception exception)
            {
                Console.WriteLine("JSON 파싱 오류: " + exception.Message);
            }
        }
    }
}
