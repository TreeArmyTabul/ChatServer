using ChatServer.Proto;
using ChatServer.Services;
using ChatServer.Utils;

namespace ChatServer.Commands
{
    public class GiftCommand : IChatCommand
    {
        private readonly InventoryRepository _inventory;
        private readonly ClientRegistry _registry;
        private readonly Func<string, ChatMessage, Task> _sendMessage;
        private readonly UserRepository _userRepo;

        public string Name => "/gift";

        public GiftCommand(InventoryRepository inventory, ClientRegistry registry, Func<string, ChatMessage, Task> sendMessage, UserRepository userRepo)
        {
            _inventory = inventory;
            _registry = registry;
            _sendMessage = sendMessage;
            _userRepo = userRepo;
        }

        public async Task ExecuteAsync(string userId)
        {
            string? senderNickname = _userRepo.GetNickname(userId);

            if (senderNickname == null)
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

                await _sendMessage(recipient.Key, new ChatMessage
                {
                    Nickname = string.Empty,
                    Type = ChatMessageType.Gift,
                    Text = $"{senderNickname}님이 {gift.Name}을(를) 보냈습니다."
                });
                return;
            }

            await _sendMessage(userId, new ChatMessage
            {
                Nickname = string.Empty,
                Type = ChatMessageType.System,
                Text = "현재는 선물을 받을 수 있는 사람이 없습니다."
            });
        }
    }
}
