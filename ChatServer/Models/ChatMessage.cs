using System.Text.Json.Serialization;

namespace ChatServer.Models
{
    public class ChatMessage
    {
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = "";
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
        [JsonPropertyName("type")]
        public string Type { get; set; } = "message";
    }
}
