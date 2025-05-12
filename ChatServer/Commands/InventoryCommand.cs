using ChatServer.Models;
using ChatServer.Proto;
using ChatServer.Services;

namespace ChatServer.Commands
{
    public class InventoryCommand : IChatCommand
    {
        private readonly InventoryRepository _inventory;
        private readonly Func<string, ChatMessage, Task> _sendMessage;

        public string Name => "/inventory";

        public InventoryCommand(InventoryRepository inventory, Func<string, ChatMessage, Task> sendMessage)
        {
            _inventory = inventory;
            _sendMessage = sendMessage;
        }

        public async Task ExecuteAsync(string userId)
        {
            List<Item> items = _inventory.GetItems(userId);

            await _sendMessage(userId, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = string.Join(", ", items.Select(item => item.Name))
            });
        }
    }
}
