using System.Security.Claims;
using Neur.Server.Net.API.Extensions;
using Serilog.Context;

namespace Neur.Server.Net.API.Middleware;

public class LoggingMiddleware {
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context) {
        var userId = context.User.FindFirst("userId")?.Value;
        var userName = context.User.FindFirst("username")?.Value;
        
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserName", userName)) {
            await _next(context);
        }
    }
}