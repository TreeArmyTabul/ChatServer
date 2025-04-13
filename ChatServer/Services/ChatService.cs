﻿using ChatServer.Commands;
using ChatServer.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ChatServer.Services
{
    public class ChatService
    {
        private readonly CommandHandler _commandHandler;
        private readonly ClientRegistry _registry;

        public ChatService(ClientRegistry registry, InventorySevice inventory)
        {
            var giftCommand = new GiftCommand(registry, inventory, SendMessageAsync);
            var inventoryCommand = new InventoryCommand(inventory, SendMessageAsync);

            _commandHandler = new CommandHandler([giftCommand, inventoryCommand]);
            _registry = registry;
        }

        public async Task AddClientAsync(WebSocket client, string nickname)
        {
            if (!_registry.TryAdd(client, nickname))
            {
                return;
            }

            Console.WriteLine($"클라이언트 연결됨: {nickname}");

            var welcomeMessage = new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Welcome,
                Text = $"환영합니다, {nickname}님!"
            };

            await SendMessageAsync(client, welcomeMessage);


            var userListExceptClient = _registry.GetOtherClients(client);

            await BroadcastMessageAsync(userListExceptClient.Select(kv => kv.Key), new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Join,
                Text = $"{nickname}님이 입장하셨습니다."
            });

            var userList = _registry.GetAllClients();

            await BroadcastMessageAsync(userList.Select(kv => kv.Key), new ChatMessage
            {
                Nickname = "System",
                Type = ChatMessageType.UserList,
                Text = string.Join(", ", userList.Select(kv => kv.Value))
            });
        }

        public async Task RemoveClientAsync(WebSocket client)
        {
            if (_registry.TryRemove(client, out var nickname))
            {
                Console.WriteLine($"클라이언트 연결 해제: {nickname}");

                var userListExceptClient = _registry.GetOtherClients(client);

                await BroadcastMessageAsync(userListExceptClient.Select(kv => kv.Key), new ChatMessage
                {
                    Nickname = nickname,
                    Type = ChatMessageType.Leave,
                    Text = $"{nickname}님이 나갔습니다."
                });
                await BroadcastMessageAsync(userListExceptClient.Select(kv => kv.Key), new ChatMessage
                {
                    Nickname = "System",
                    Type = ChatMessageType.UserList,
                    Text = string.Join(", ", userListExceptClient.Select(kv => kv.Value))
                });
            }
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
                if (await _commandHandler.TryHandleAsync(client, message.Text))
                {
                    return;
                }

                var userListExceptClient = _registry.GetOtherClients(client);

                await BroadcastMessageAsync(userListExceptClient.Select(kv => kv.Key), message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("JSON 파싱 오류: " + exception.Message);
            }
        }

        private async Task SendMessageAsync(WebSocket recipient, ChatMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);

                await recipient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Console.WriteLine("메시지 전송 오류: " + exception.Message);
            }
        }
        
        private async Task BroadcastMessageAsync(IEnumerable<WebSocket> recipients, ChatMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);

                foreach (var recipient in recipients)
                {
                    await recipient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("브로드캐스트 오류: " + exception.Message);
            }
        }
    }
}
