using ChatServer.Commands;

namespace ChatServer.Services
{
    public class CommandHandler
    {
        private readonly Dictionary<string, IChatCommand> _commands = new();

        public CommandHandler(IEnumerable<IChatCommand> commands)
        {
            _commands = commands.ToDictionary(cmd => cmd.Name, cmd => cmd);
        }
    
        public async Task<bool> TryHandleAsync(string userId, string text)
        {
            if (_commands.TryGetValue(text, out var command))
            {
                await command.ExecuteAsync(userId);
                return true;
            }
            return false;
        }
    }
}
