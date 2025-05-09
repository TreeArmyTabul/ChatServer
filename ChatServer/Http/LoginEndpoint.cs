using System.Text.Json;
using ChatServer.Services;

namespace ChatServer.Http
{
    public static class LoginEndpoint
    {
        private record LoginRequest(string Id, string Password);

        public static void MapLogin(this WebApplication app, TokenService tokenService, UserRepository userRepo)
        {
            app.MapPost("/login", async (HttpContext context) =>
            {
                try
                {
                    LoginRequest? request = await JsonSerializer.DeserializeAsync<LoginRequest>(context.Request.Body);

                    if (request is null || string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Password))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { success = false, message = "잘못된 요청" });
                        return;
                    }

                    bool success = userRepo.Login(request.Id, request.Password);

                    if (!success)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new { success = false, message = "아이디 또는 비밀번호 불일치" });
                        return;
                    }

                    string token = tokenService.Issue(request.Id);
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsJsonAsync(new { success = true, message = string.Empty, token });
                }
                catch (Exception ex) { 
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = $"서버 오류: {ex.Message}" }   );
                }
            });
        }
    }
}
