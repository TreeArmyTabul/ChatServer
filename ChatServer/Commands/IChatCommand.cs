using System.Net.WebSockets;

namespace ChatServer.Commands
{
    public interface IChatCommand
    {
        string Name { get; }
        Task ExecuteAsync(string userId);
    }
}
