using ChatServer.Http;
using ChatServer.Services;
using System.Net.WebSockets;

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

app.MapRegister(userRepository); // "/register" 엔드포인트를 등록합니다.

var tokenService = new TokenService();

app.MapLogin(tokenService, userRepository); // "/login" 엔드포인트를 등록합니다.

var clientRegistry = new ClientRegistry();
var inventoryService = new InventoryRepository();
var chatService = new ChatService(clientRegistry, inventoryService, userRepository);

app.Map("/chat", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        string? token = context.Request.Query["token"].FirstOrDefault();
        string format = context.Request.Query["format"].FirstOrDefault() ?? "json";

        if (format != "json" && format != "protobuf")
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("지원하지 않는 포맷입니다.");
            return;
        }

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

        await chatService.AddClientAsync(client, userId!, nickname, format);

        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "연결 해제", CancellationToken.None);
                await chatService.RemoveClientAsync(userId!);
                break;
            }

            await chatService.HandleMessageAsync(userId!, buffer[..receivedResult.Count]);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
