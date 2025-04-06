using ChatServer.Services;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var chatService = new ChatService();

app.UseWebSockets();

app.Map("/chat", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var client = await context.WebSockets.AcceptWebSocketAsync();
        chatService.AddClient(client);

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "연결 해제", CancellationToken.None);
                chatService.RemoveClient(client);
                break;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
            Console.WriteLine($"받은 메시지: {json}");

            await chatService.BroadcastMessageAsync(json, client);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
