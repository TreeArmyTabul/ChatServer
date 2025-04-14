using ChatServer.Http;
using ChatServer.Services;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

app.UseCors();
app.UseWebSockets();

var userRepository = new UserRepository();

app.MapRegister(userRepository); // "/register" ��������Ʈ�� ����մϴ�.

var tokenService = new TokenService();

app.MapLogin(tokenService, userRepository); // "/login" ��������Ʈ�� ����մϴ�.

var clientRegistry = new ClientRegistry();
var inventoryService = new InventorySevice();
var chatService = new ChatService(clientRegistry, inventoryService, userRepository);

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

        await chatService.AddClientAsync(client, userId!, nickname);

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "���� ����", CancellationToken.None);
                await chatService.RemoveClientAsync(userId!);
                break;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
            Console.WriteLine($"���� �޽���: {json}");

            await chatService.HandleMessageAsync(userId!, json);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
