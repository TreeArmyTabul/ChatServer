using System.Net.WebSockets;
using System.Text;

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

        public async Task BroadcastMessageAsync(string message, WebSocket sender)
        {
            var buffer = Encoding.UTF8.GetBytes(message);

            foreach (var otherClient in _clients)
            {
                if (otherClient != sender && otherClient.State == WebSocketState.Open)
                {
                    await otherClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
