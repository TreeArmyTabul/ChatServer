using ChatServer.Commands;
using System.Net.WebSockets;

namespace ChatServer.Services
{
    public class CommandHandler
    {
        private readonly Dictionary<string, IChatCommand> _commands = new();

        public CommandHandler(IEnumerable<IChatCommand> commands)
        {
            _commands = commands.ToDictionary(cmd => cmd.Name, cmd => cmd);
        }
    
        public async Task<bool> TryHandleAsync(WebSocket sender, string text)
        {
            if (_commands.TryGetValue(text, out var command))
            {
                await command.ExecuteAsync(sender);
                return true;
            }
            return false;
        }
    }
}
