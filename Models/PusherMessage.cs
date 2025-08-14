using System.Text.Json.Serialization;

namespace KickChatSpy.Models
{
    public class PusherMessage
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}