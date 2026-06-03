using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Infrastructure.Logging;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            using (Serilog.Context.LogContext.PushProperty("UserId", userContext.UserId))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
