using Serilog;
using Serilog.Events;

namespace Neur.Server.Net.API.Extensions;

public static class LoggingExtensions {
    public static void AddLogging(this WebApplicationBuilder builder) {
        var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {UserName} {UserId} {NewLine}{Exception}";
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File("logs/app.log", outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}