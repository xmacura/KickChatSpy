using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace KickChatSpy;

internal sealed class ChatroomLookupService
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://kick-auth-bridge-hdhdftd5a7dth5ge.westeurope-01.azurewebsites.net")
    };

    public async Task<int?> GetChatroomIdAsync(string channelname)
    {
        if (string.IsNullOrWhiteSpace(channelname))
            throw new ArgumentException("Channel name is required.", nameof(channelname));

        var resp = await _http.GetFromJsonAsync<ChatroomApiResponse>(
            $"/api/chatroom/{Uri.EscapeDataString(channelname)}");

        return resp?.ChatroomId;
    }

    private sealed class ChatroomApiResponse
    {
        [JsonPropertyName("chatroomId")] public int ChatroomId { get; set; }
        [JsonPropertyName("cached")] public bool Cached { get; set; }
    }
}