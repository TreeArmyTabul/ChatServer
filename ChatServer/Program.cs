using ChatServer.Http;
using ChatServer.Services;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var userRepository = new UserRepository();

app.MapRegister(userRepository); // "/register" ��������Ʈ�� ����մϴ�.

var tokenService = new TokenService();

app.MapLogin(tokenService, userRepository); // "/login" ��������Ʈ�� ����մϴ�.

var clientRegistry = new ClientRegistry();
var inventoryService = new InventorySevice();
var chatService = new ChatService(clientRegistry, inventoryService);

app.UseWebSockets();

app.Map("/chat", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        string? token = context.Request.Query["token"].FirstOrDefault();

        // 1. ��ū ��ȿ�� �˻�
        if (token is null || !tokenService.Validate(token, out string? userId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("���ε��� ���� ����");
            return;
        }

        // 2. ����� ���� ��ȸ
        string? nickname = userRepository.GetNickname(userId!);

        if (nickname is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("���ε��� ���� ����");
            return;
        }

        // 3. WeobSocket ����
        var client = await context.WebSockets.AcceptWebSocketAsync();

        // 4. ��ū ���� ó��
        tokenService.Revoke(token);

        await chatService.AddClientAsync(client, nickname);

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "���� ����", CancellationToken.None);
                await chatService.RemoveClientAsync(client);
                break;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
            Console.WriteLine($"���� �޽���: {json}");

            await chatService.HandleMessageAsync(client, json);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
