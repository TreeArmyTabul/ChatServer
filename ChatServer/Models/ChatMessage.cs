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
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatMessageType Type { get; set; }
    }
}
