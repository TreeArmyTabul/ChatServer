using System.Net.WebSockets;
using ChatServer.Models;
using ChatServer.Services;

namespace ChatServer.Commands
{
    public class InventoryCommand : IChatCommand
    {
        private readonly InventorySevice _inventory;
        private readonly Func<WebSocket, ChatMessage, Task> _sendMessage;

        public string Name => "/inventory";

        public InventoryCommand(InventorySevice inventory, Func<WebSocket, ChatMessage, Task> sendMessage)
        {
            _inventory = inventory;
            _sendMessage = sendMessage;
        }

        public async Task ExecuteAsync(WebSocket sender)
        {
            var items = _inventory.GetItems(sender);

            await _sendMessage(sender, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = string.Join(", ", items)
            });
        }
    }
}
