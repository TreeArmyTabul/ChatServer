using System.Text.Json;
using ChatServer.Services;

namespace ChatServer.Http
{
    public static class RegisterEndpoint
    {
        private record RegisterRequest(string Id, string Password);

        public static void MapRegister(this WebApplication app, UserRepository userRepo)
        {
            app.MapPost("/register", async (HttpContext context) =>
            {
                try
                {
                    RegisterRequest? request = await JsonSerializer.DeserializeAsync<RegisterRequest>(context.Request.Body);

                    if (request is null || string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Password))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { success = false, message = "잘못된 요청" });
                        return;
                    }

                    bool success = userRepo.Register(request.Id, request.Password);

                    if (!success)
                    {
                        context.Response.StatusCode = 409;
                        await context.Response.WriteAsJsonAsync(new { success = false, message = "회원가입 실패" });
                        return;
                    }

                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsJsonAsync(new { success = true, message = "회원가입 성공" });
                }
                catch (Exception ex) { 
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = $"서버 오류: {ex.Message}" });
                }
            });
        }
    }
}
