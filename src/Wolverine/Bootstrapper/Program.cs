using Serilog;
using Serilog.Events;

// Configure Serilog as the logging provider
// 配置 Serilog 作为日志提供程序
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#if DEBUG
    .MinimumLevel.Override("Zss.BilliardHall", LogEventLevel.Debug)
    .MinimumLevel.Override("Wolverine", LogEventLevel.Debug)
    .MinimumLevel.Override("Marten", LogEventLevel.Information)
#else
    .MinimumLevel.Override("Zss.BilliardHall", LogEventLevel.Information)
    .MinimumLevel.Override("Wolverine", LogEventLevel.Information)
    .MinimumLevel.Override("Marten", LogEventLevel.Warning)
#endif
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BilliardHall.Bootstrapper")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Billiard Hall application...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add Aspire ServiceDefaults (OpenTelemetry, Health Checks, Service Discovery)
    builder.AddServiceDefaults();

    // Add Marten document database
    builder.AddMartenDefaults();

    // Add Wolverine command/message bus with HTTP endpoints
    builder.AddWolverineDefaults();

    var app = builder.Build();

    // Map health check endpoints (/health, /alive)
    app.MapDefaultEndpoints();

    app.MapGet("/", () => new
    {
        Application = "Zss.BilliardHall.Bootstrapper",
        Status = "Running",
        Framework = "Wolverine + Marten",
        Architecture = "Vertical Slice"
    });

    Log.Information("Application started successfully");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
