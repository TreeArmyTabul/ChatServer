using ChatServer.Commands;
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
        private readonly UserRepository _userRepository;

        public ChatService(ClientRegistry registry, InventoryRepository inventory, UserRepository userRepository)
        {
            var giftCommand = new GiftCommand(inventory, registry, SendMessageAsync, userRepository);
            var inventoryCommand = new InventoryCommand(inventory, registry, SendMessageAsync);

            _commandHandler = new CommandHandler([giftCommand, inventoryCommand]);
            _registry = registry;
            _userRepository = userRepository;
        }

        public async Task AddClientAsync(WebSocket client, string userId, string nickname)
        {
            await _registry.RegisterAsync(userId, client);

            Console.WriteLine($"클라이언트 연결됨: {nickname}");

            await SendMessageAsync(client, new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Welcome,
                Text = $"환영합니다, {nickname}님!"
            });

            var others = _registry.GetSocketsExcept(userId);

            await BroadcastMessageAsync(others.Select(pair => pair.Value), new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Join,
                Text = $"{nickname}님이 입장하셨습니다."
            });

            var all = _registry.GetAllSockets();
            var nicknames = all
                .Select(pair => _userRepository.GetNickname(pair.Key))
                .Where(nickname => nickname != null);

            await BroadcastMessageAsync(all.Select(pair => pair.Value), new ChatMessage
            {
                Nickname = "System",
                Type = ChatMessageType.UserList,
                Text = string.Join(", ", nicknames)
            });
        }

        public async Task RemoveClientAsync(string userId)
        {
            if (!_registry.TryRemove(userId))
            {
                return;
            }

            string? nickname = _userRepository.GetNickname(userId);

            if (nickname == null)
            {
                return;
            }

            Console.WriteLine($"클라이언트 연결 해제: {nickname}");

            var others = _registry.GetSocketsExcept(userId);

            await BroadcastMessageAsync(others.Select(pair => pair.Value), new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Leave,
                Text = $"{nickname}님이 나갔습니다."
            });

            var nicknames = others
                .Select(pair => _userRepository.GetNickname(pair.Key))
                .Where(nickname => nickname != null);

            await BroadcastMessageAsync(others.Select(pair => pair.Value), new ChatMessage
            {
                Nickname = "System",
                Type = ChatMessageType.UserList,
                Text = string.Join(", ", nicknames)
            });
        }

        public async Task HandleMessageAsync(string userId, string json)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(json);
                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                {
                    return;
                }
                if (await _commandHandler.TryHandleAsync(userId, message.Text))
                {
                    return;
                }

                var others = _registry.GetSocketsExcept(userId);

                await BroadcastMessageAsync(others.Select(pair => pair.Value), message);
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
