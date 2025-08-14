using System.Text.Json.Serialization;

namespace KickChatSpy.Models
{
    public class Sender
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("identity")]
        public Identity Identity { get; set; }
    }
}