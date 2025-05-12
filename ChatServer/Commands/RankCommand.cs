using ChatServer.Proto;
using ChatServer.Services;

namespace ChatServer.Commands
{
    public class RankCommand : IChatCommand
    {
        private readonly InventoryRepository _inventory;
        private readonly Func<string, ChatMessage, Task> _sendMessage;
        private readonly UserRepository _userRepo;

        public string Name => "/rank";

        public RankCommand(InventoryRepository inventory, Func<string, ChatMessage, Task> sendMessage, UserRepository userRepo)
        {
            _inventory = inventory;
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

            await _sendMessage(userId, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = string.Join("\n", scores)
            });
        }
    }
}
