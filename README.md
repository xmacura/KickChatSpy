# KickChatSpy

**KickChatSpy** is a lightweight C# library for listening to live chat messages from [Kick.com](https://kick.com) using a WebSocket connection.

## Installation

You can install the package via NuGet:

```bash
dotnet add package KickChatSpy
```

## Example

Subscribe by channel name:

```csharp
using KickChatSpy;

var kickChat = new KickChatClient();

kickChat.OnMessageReceived += (msg) =>
{
    Console.WriteLine($"[{msg.CreatedAt:T}] {msg.Sender.Username}: {msg.Content}");
};

var channelname = "chrimsie";
await kickChat.ConnectToChatroomAsync(channelname);
```

Or subscribe directly by chatroom ID:

```csharp
var chatroomId = 692373;
await kickChat.ConnectToChatroomAsync(chatroomId);
```

## License

[MIT License](LICENSE)
