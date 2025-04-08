﻿using ChatServer.Models;
using ChatServer.Utils;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ChatServer.Services
{
    public class ChatService
    {
        private readonly Dictionary<WebSocket, string> _clients = new();

        public async Task AddClientAsync(WebSocket client)
        {
            if (_clients.ContainsKey(client))
            {
                return;
            }

            var nickname = NicknameGenerator.Generate();

            _clients[client] = nickname;
            Console.WriteLine($"클라이언트 연결됨: {nickname}");

            var welcomeMessage = new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Welcome,
                Text = $"환영합니다, {nickname}님!"
            };

            await SendMessageAsync(client, welcomeMessage);

            var joinMessage = new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Join,
                Text = $"{nickname}님이 입장하셨습니다."
            };

            await BroadcastMessageAsync(client, joinMessage);
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

            await BroadcastMessageAsync(client, message);
        }

        public async Task HandleMessageAsync(WebSocket client, string json)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(json);
                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                {
                    return;
                }
                await BroadcastMessageAsync(client, message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("JSON 파싱 오류: " + exception.Message);
            }
        }

        private async Task SendMessageAsync(WebSocket client, ChatMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                var buffer = Encoding.UTF8.GetBytes(json);

                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Console.WriteLine("메시지 전송 오류: " + exception.Message);
            }
        }

        private async Task BroadcastMessageAsync(WebSocket sender, ChatMessage message)
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
                Console.WriteLine("브로드캐스트 오류: " + exception.Message);
            }
        }
    }
}
