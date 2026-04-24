using Neur.Server.Net.API.Contracts;
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
        if (context.Response.HasStarted)
            return Task.CompletedTask;

        context.Response.ContentType = "application/json";

        if (ex is BaseException) {
            var baseException = ex as BaseException;
            if (baseException != null) {
                context.Response.StatusCode = baseException.StatusCode;
                var response = new ServerErrorResponse(
                    Status: baseException.StatusCode,
                    Error: baseException.Message
                );
                return context.Response.WriteAsJsonAsync(response);
            }
        }
        context.Response.StatusCode = 500;
        return context.Response.WriteAsJsonAsync(
            new ServerErrorResponse(
                Status: 500,
                Error: "Internal server error"
            )
        );
    }
}