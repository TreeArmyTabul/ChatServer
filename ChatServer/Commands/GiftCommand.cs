using System.Net.WebSockets;
using ChatServer.Models;
using ChatServer.Services;
using ChatServer.Utils;

namespace ChatServer.Commands
{
    public class GiftCommand : IChatCommand
    {
        private readonly InventoryRepository _inventory;
        private readonly ClientRegistry _registry;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;
        private readonly UserRepository _userRepo;

        public string Name => "/gift";

        public GiftCommand(InventoryRepository inventory, ClientRegistry registry, Func<WebSocket, ChatMessage, Task> sendMessage, UserRepository userRepo)
        {
            _inventory = inventory;
            _registry = registry;
            _sendMessage = sendMessage;
            _userRepo = userRepo;
        }

        public async Task ExecuteAsync(string userId)
        {
            WebSocket? socket = _registry.GetSocketByUserId(userId);
            string? senderNickname = _userRepo.GetNickname(userId);

            if (socket == null || senderNickname == null)
            {
                return;
            }

            var candidates = _registry.GetSocketsExcept(userId).ToList();

            if (candidates.Count > 0)
            {
                var random = new Random();
                var recipient = candidates[random.Next(candidates.Count)];
                var gift = GiftGenerator.GetRandomGift();

                _inventory.AddItem(recipient.Key, gift);

                await _sendMessage(recipient.Value, new ChatMessage
                {
                    Nickname = string.Empty,
                    Type = ChatMessageType.Gift,
                    Text = $"{senderNickname}님이 {gift.Name}을(를) 보냈습니다."
                });
                return;
            }

            await _sendMessage(socket, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = "현재는 선물을 받을 수 있는 사람이 없습니다."
            });
        }
    }
}
