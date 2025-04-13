using ChatServer.Models;
using ChatServer.Utils;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class CommandHandler
    {
        private readonly InventorySevice _inventory;
        private readonly ClientRegistry _registry;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;

        public CommandHandler(InventorySevice inventory, ClientRegistry registry, Func<WebSocket, ChatMessage, Task> send)
        {
            _inventory = inventory;
            _registry = registry;
            _sendMessage = send;
        }
    
        public async Task<bool> TryHandleAsync(WebSocket sender, string text)
        {
            if (text.Trim() == "/gift")
            { 
                if (!_registry.TryGetNickname(sender, out var senderNickname))
                {
                    return true;
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

                    return true;
                }

                var gift = GiftGenerator.GetRandomGift();

                _inventory.AddItem(recipient, gift);

                await _sendMessage(recipient, new ChatMessage
                {
                    Nickname = senderNickname,
                    Type = ChatMessageType.Gift,
                    Text = $"{senderNickname}님이 {gift}을(를) 보냈습니다."
                });
                
                return true;
            }
            if (text.Trim() == "/inventory")
            {
                var items = _inventory.GetItems(sender);

                await _sendMessage(sender, new ChatMessage
                {
                    Nickname = string.Empty,
                    Type = ChatMessageType.System,
                    Text = string.Join(", ", items)
                });

                return true;
            }
            return false;
        }
    }
}
