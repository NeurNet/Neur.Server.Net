using Neur.Server.Net.Application.Exeptions;

namespace Neur.Server.Net.API.Middleware;

public class MiddlewareHandler {
    private readonly RequestDelegate _next;
    private readonly ILogger<MiddlewareHandler> _logger;

    public MiddlewareHandler(RequestDelegate next, ILogger<MiddlewareHandler> logger) {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex) {
        context.Response.ContentType = "application/json";

        if (ex is BaseException) {
            var baseException = ex as BaseException;
            if (baseException != null) {
                context.Response.StatusCode = baseException.StatusCode;
                var response = new {
                    error = baseException.Message,
                    status = baseException.StatusCode,
                };
                return context.Response.WriteAsJsonAsync(response);
            }
        }
        context.Response.StatusCode = 500;
        return context.Response.WriteAsJsonAsync(new {
            error = "Internal server error",
            status = 500
        });
    }
}