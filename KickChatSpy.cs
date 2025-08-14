using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using KickChatSpy.Models;

namespace KickChatSpy;

public class KickChatSpy
{
    private static readonly Uri WebSocketUri = new("wss://ws-us2.pusher.com/app/32cbd69e4b950bf97679");

    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;

    public bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;
    public event Action<ChatMessage> OnMessageReceived;

    public async Task ConnectToChatroomAsync(long chatroomNumber)
    {
        if (IsConnected)
            throw new InvalidOperationException("Already connected.");

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        await _ws.ConnectAsync(WebSocketUri, _cts.Token);
        Console.WriteLine($"Connected to WebSocket for chatroom: {chatroomNumber}");

        await SubscribeToChannelsAsync(chatroomNumber);

        _ = Task.Run(() => StartReceivingMessagesAsync(_cts.Token));
    }

    public async Task DisconnectAsync()
    {
        if (!IsConnected) return;

        _cts?.Cancel();

        try
        {
            await _ws!.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
        }
        catch
        {
            Console.WriteLine("Error while closing WebSocket connection.");
        }

        _ws?.Dispose();
        _ws = null;
        _cts?.Dispose();
        _cts = null;

        Console.WriteLine("Disconnected from chatroom.");
    }

    private async Task SubscribeToChannelsAsync(long? chatroomNumber)
    {
        if (!chatroomNumber.HasValue)
            throw new ArgumentException("Invalid chatroom number.", nameof(chatroomNumber));

        string[] channels =
        [
            $"chatroom_{chatroomNumber}",
            $"chatrooms.{chatroomNumber}.v2"
        ];

        foreach (var channel in channels)
        {
            var payload = new
            {
                @event = "pusher:subscribe",
                data = new
                {
                    auth = "",
                    channel = channel
                }
            };

            string json = JsonSerializer.Serialize(payload);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

            await _ws.SendAsync(buffer, WebSocketMessageType.Text, true, _cts!.Token);
            Console.WriteLine($"Subscribed to: {channel}");
        }
    }

    private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        while (!cancellationToken.IsCancellationRequested && _ws?.State == WebSocketState.Open)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var pusherMsg = JsonSerializer.Deserialize<PusherMessage>(json);
                if (pusherMsg?.Event?.Contains("ChatMessageEvent") == true)
                {
                    var chatMsg = JsonSerializer.Deserialize<ChatMessage>(pusherMsg.Data);
                    if (chatMsg != null)
                    {
                        OnMessageReceived?.Invoke(chatMsg);
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[JSON] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReceiveError] {ex.Message}");
            }
        }

        Console.WriteLine("Stopped receiving messages.");
    }
}
