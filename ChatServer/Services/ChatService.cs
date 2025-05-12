using ChatServer.Commands;
using ChatServer.Proto;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Google.Protobuf;

namespace ChatServer.Services
{
    public class ChatService
    {
        private readonly CommandHandler _commandHandler;
        private readonly ClientRegistry _registry;
        private readonly UserRepository _userRepository;
        private readonly Dictionary<string, string> _clientFormats = new();

        public ChatService(ClientRegistry registry, InventoryRepository inventory, UserRepository userRepository)
        {
            var giftCommand = new GiftCommand(inventory, registry, SendMessageAsync, userRepository);
            var inventoryCommand = new InventoryCommand(inventory, SendMessageAsync);
            var rankCommand = new RankCommand(inventory, SendMessageAsync, userRepository);

            _commandHandler = new CommandHandler([giftCommand, inventoryCommand, rankCommand]);
            _registry = registry;
            _userRepository = userRepository;
        }

        public async Task AddClientAsync(WebSocket client, string userId, string nickname, string format)
        {
            _clientFormats[userId] = format;
            await _registry.RegisterAsync(userId, client);

            Console.WriteLine($"클라이언트 연결됨: {nickname} (포맷: {format})");

            await SendMessageAsync(userId, new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Welcome,
                Text = $"환영합니다, {nickname}님!"
            });

            await BroadcastMessageAsync(new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Join,
                Text = $"{nickname}님이 입장하셨습니다."
            }, userId);

            var all = _registry.GetAllSockets();
            var nicknames = all
                .Select(pair => _userRepository.GetNickname(pair.Key))
                .Where(nickname => nickname != null);

            await BroadcastMessageAsync(new ChatMessage
            {
                Nickname = "System",
                Type = ChatMessageType.UserList,
                Text = string.Join(", ", nicknames)
            }, null);
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

            await BroadcastMessageAsync(new ChatMessage
            {
                Nickname = nickname,
                Type = ChatMessageType.Leave,
                Text = $"{nickname}님이 나갔습니다."
            }, userId);

            var others = _registry.GetAllSockets();
            var nicknames = others
                .Select(pair => _userRepository.GetNickname(pair.Key))
                .Where(nickname => nickname != null);

            await BroadcastMessageAsync(new ChatMessage
            {
                Nickname = "System",
                Type = ChatMessageType.UserList,
                Text = string.Join(", ", nicknames)
            }, null);
        }

        public async Task HandleMessageAsync(string userId, byte[] data)
        {
            try
            {
                ChatMessage message;
                if (_clientFormats.TryGetValue(userId, out var format) && format == "protobuf")
                {
                    message = ChatMessage.Parser.ParseFrom(data);
                }
                else
                {
                    var json = Encoding.UTF8.GetString(data);
                    message = JsonSerializer.Deserialize<ChatMessage>(json);
                }

                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                {
                    return;
                }
                
                try
                {
                    if (await _commandHandler.TryHandleAsync(userId, message.Text))
                    {
                        return;
                    }
                }
                catch (CommandNotFoundException exception)
                {
                    await SendMessageAsync(userId, new ChatMessage
                    {
                        Nickname = "System",
                        Type = ChatMessageType.System,
                        Text = exception.Message
                    });
                }

                await BroadcastMessageAsync(message, userId);
            }
            catch (Exception exception)
            {
                Console.WriteLine("메시지 파싱 오류: " + exception.Message);
            }
        }

        private async Task SendMessageAsync(string userId, ChatMessage message)
        {
            try
            {
                var recipient = _registry.GetSocketByUserId(userId);

                if (recipient == null)
                {
                    return;
                }

                if (_clientFormats.TryGetValue(userId, out var format) && format == "protobuf")
                {
                    var protoMessage = new ChatMessage
                    {
                        Type = message.Type,
                        Nickname = message.Nickname,
                        Text = message.Text
                    };
                    var buffer = protoMessage.ToByteArray();
                    await recipient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
                else
                {
                    var json = JsonSerializer.Serialize(message);
                    var buffer = Encoding.UTF8.GetBytes(json);
                    await recipient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("메시지 전송 오류: " + exception.Message);
            }
        }
        
        private async Task BroadcastMessageAsync(ChatMessage message, string? sender)
        {
            var recipients = _registry.GetAllSockets()
                .Where((kv) => kv.Key != sender)
                .Select((kv) => kv.Key);

            foreach (var recipient in recipients)
            {
                await SendMessageAsync(recipient, message);
            }
        }
    }
}
