using ChatServer.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ChatServer.Services
{
    public class ChatService
    {
        private readonly Dictionary<WebSocket, string> _clients = new();

        private void AddClient(WebSocket client, string nickname)
        {
            if (_clients.ContainsKey(client))
            {
                return;
            }

            _clients[client] = nickname;
            Console.WriteLine($"클라이언트 연결됨: {nickname}");
        }

        public async Task RemoveClientAsync(WebSocket client)
        {
            _clients.TryGetValue(client, out var nickname);

            if (nickname == null)
            {
                return;
            }

            _clients.Remove(client);
            Console.WriteLine($"클라이언트 연결 해제: {nickname}");

            var message = new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Leave,
                Text = $"{nickname}님이 나갔습니다."
            };

            await BroadcastMessageAsync(message, client);
        }

        public async Task HandleMessageAsync(string json, WebSocket client)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(json);

                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                {
                    return;
                }

                if (message.Type == ChatMessageType.Join)
                {
                    AddClient(client, message.Nickname);
                }

                await BroadcastMessageAsync(message, client);
            }
            catch (Exception exception)
            {
                Console.WriteLine("JSON 파싱 오류: " + exception.Message);
            }
        }

        private async Task BroadcastMessageAsync(ChatMessage message, WebSocket sender)
        {
            try
            {
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                var buffer = Encoding.UTF8.GetBytes(json);

                foreach (var otherClient in _clients)
                {
                    if (otherClient.Key != sender && otherClient.Key.State == WebSocketState.Open)
                    {
                        await otherClient.Key.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            } catch (Exception exception)
            {
                Console.WriteLine("JSON 파싱 오류: " + exception.Message);
            }
        }
    }
}
