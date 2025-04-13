using ChatServer.Models;
using ChatServer.Utils;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class CommandHandler
    {
        private readonly ClientRegistry _registry;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;

        public CommandHandler(ClientRegistry registry, Func<WebSocket, ChatMessage, Task> send)
        {
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

                if (recipient == null )
                {
                    var systemMessage = new ChatMessage
                    {
                        Nickname = senderNickname,
                        Type = ChatMessageType.System,
                        Text = "현재는 선물을 받을 수 있는 사람이 없습니다."
                    };

                    await _sendMessage(sender, systemMessage);
                    return true;
                }
                var gift = GiftGenerator.GetRandomGift();
                var giftMessage = new ChatMessage
                {
                    Nickname = senderNickname,
                    Type = ChatMessageType.Gift,
                    Text = $"{senderNickname}님이 {gift}을(를) 보냈습니다."
                };

                await _sendMessage(recipient, giftMessage);
                return true;
            }
            return false;
        }
    }
}
