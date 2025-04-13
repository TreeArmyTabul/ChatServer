namespace ChatServer.Models
{
    public class ChatMessage
    {
        public string Nickname { get; set; } = "";
        public string Text { get; set; } = "";
        public ChatMessageType Type { get; set; }
    }
}
