using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var clients = new List<WebSocket>();

app.UseWebSockets();

app.Map("/chat", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var client = await context.WebSockets.AcceptWebSocketAsync();
        clients.Add(client);
        Console.WriteLine("클라이언트 연결.");

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "연결 해제", CancellationToken.None);
                Console.WriteLine("클라이언트 연결 해제.");
                clients.Remove(client);
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
            Console.WriteLine($"받은 메시지: {message}");


            // Broadcast the message to all connected clients expluding the sender
            foreach (var otherClient in clients)
            {
                if (otherClient != client && otherClient.State == WebSocketState.Open)
                {
                    await otherClient.SendAsync(new ArraySegment<byte>(buffer, 0, receivedResult.Count), receivedResult.MessageType, receivedResult.EndOfMessage, CancellationToken.None);
                }
            }
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
