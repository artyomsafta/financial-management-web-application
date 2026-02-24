using Serilog;
using Serilog.Events;

namespace Task11_DotNETBackendWebApi.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override(source: "Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override(source: "System", LogEventLevel.Warning)
            .MinimumLevel.Override(source: "Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(path: @"Logs/Task11WebApi-.txt", 
                rollingInterval: RollingInterval.Day, 
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} | CorrelationId: {CorrelationId} | User: {UserId}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
