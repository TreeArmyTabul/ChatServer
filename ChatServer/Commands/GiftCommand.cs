using System.Net.WebSockets;
using ChatServer.Models;
using ChatServer.Services;
using ChatServer.Utils;

namespace ChatServer.Commands
{
    public class GiftCommand : IChatCommand
    {
        private readonly ClientRegistry _registry;
        private readonly InventorySevice _inventory;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;

        public string Name => "/gift";

        public GiftCommand(ClientRegistry registry, InventorySevice inventory, Func<WebSocket, ChatMessage, Task> sendMessage)
        {
            _registry = registry;
            _inventory = inventory;
            _sendMessage = sendMessage;
        }

        public async Task ExecuteAsync(WebSocket sender)
        {
            if (!_registry.TryGetNickname(sender, out var senderNickname))
            {
                return;
            }

            var recipient = _registry.GetRandomClient(sender);

            if (recipient == null)
            {
                var systemMessage = new ChatMessage
                {
                    Nickname = string.Empty,
                    Type = ChatMessageType.System,
                    Text = "현재는 선물을 받을 수 있는 사람이 없습니다."
                };

                await _sendMessage(sender, systemMessage);

                return;
            }

            var gift = GiftGenerator.GetRandomGift();

            _inventory.AddItem(recipient, gift);

            await _sendMessage(recipient, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.Gift,
                Text = $"{senderNickname}님이 {gift}을(를) 보냈습니다."
            });
        }
    }
}
