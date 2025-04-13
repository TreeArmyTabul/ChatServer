using ChatServer.Http;
using ChatServer.Services;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var userRepository = new UserRepository();

app.MapRegister(userRepository); // "/register" 엔드포인트를 등록합니다.

var tokenService = new TokenService();

app.MapLogin(tokenService, userRepository); // "/login" 엔드포인트를 등록합니다.

var clientRegistry = new ClientRegistry();
var inventoryService = new InventorySevice();
var chatService = new ChatService(clientRegistry, inventoryService);

app.UseWebSockets();

app.Map("/chat", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        string? token = context.Request.Query["token"].FirstOrDefault();

        // 1. 토큰 유효성 검사
        if (token is null || !tokenService.Validate(token, out string? userId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("승인되지 않은 접근");
            return;
        }

        // 2. 사용자 정보 조회
        string? nickname = userRepository.GetNickname(userId!);

        if (nickname is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("승인되지 않은 접근");
            return;
        }

        // 3. WeobSocket 연결
        var client = await context.WebSockets.AcceptWebSocketAsync();

        // 4. 토큰 만료 처리
        tokenService.Revoke(token);

        await chatService.AddClientAsync(client, nickname);

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "연결 해제", CancellationToken.None);
                await chatService.RemoveClientAsync(client);
                break;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
            Console.WriteLine($"받은 메시지: {json}");

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
