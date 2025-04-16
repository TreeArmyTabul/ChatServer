using ChatServer.Models;
using ChatServer.Services;
using System.Net.WebSockets;

namespace ChatServer.Commands
{
    public class RankCommand : IChatCommand
    {
        private readonly InventoryRepository _inventory;
        private readonly ClientRegistry _registry;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;
        private readonly UserRepository _userRepo;

        public string Name => "/rank";

        public RankCommand(InventoryRepository inventory, ClientRegistry registry, Func<WebSocket, ChatMessage, Task> sendMessage, UserRepository userRepo)
        {
            _inventory = inventory;
            _registry = registry;
            _sendMessage = sendMessage;
            _userRepo = userRepo;
        }

        public async Task ExecuteAsync(string userId)
        {
            var scores = _inventory.GetUserValues()
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select((kv, index) =>
                {
                    var nickname = _userRepo.GetNickname(kv.Key) ?? kv.Key;
                    return $"{index + 1}위. {nickname} - {kv.Value}점";
                });

            WebSocket? socket = _registry.GetSocketByUserId(userId);

            if (socket == null)
            {
                return;
            }

            await _sendMessage(socket, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = string.Join("\n", scores)
            });
        }
    }
}
