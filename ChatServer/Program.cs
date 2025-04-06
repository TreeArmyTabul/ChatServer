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
        Console.WriteLine("Ŭ���̾�Ʈ ����.");

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "���� ����", CancellationToken.None);
                Console.WriteLine("Ŭ���̾�Ʈ ���� ����.");
                clients.Remove(client);
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
            Console.WriteLine($"���� �޽���: {message}");


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
