using System.Net.WebSockets;
using ChatServer.Models;
using ChatServer.Services;

namespace ChatServer.Commands
{
    public class InventoryCommand : IChatCommand
    {
        private readonly InventorySevice _inventory;
        private readonly ClientRegistry _registry;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;

        public string Name => "/inventory";

        public InventoryCommand(InventorySevice inventory, ClientRegistry registry, Func<WebSocket, ChatMessage, Task> sendMessage)
        {
            _inventory = inventory;
            _registry = registry;
            _sendMessage = sendMessage;
        }

        public async Task ExecuteAsync(string userId)
        {
            WebSocket? socket = _registry.GetSocketByUserId(userId);

            if (socket == null) {
                return;
            }

            List<string> items = _inventory.GetItems(userId);

            await _sendMessage(socket, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = string.Join(", ", items)
            });
        }
    }
}
