using Serilog;

// Create a bootstrap logger for early startup logging
// 创建引导日志记录器用于早期启动日志
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Billiard Hall application...");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings.json
    // 从 appsettings.json 配置 Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BilliardHall.Bootstrapper")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

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

    Log.Information("Application configured successfully, starting web server...");

    app.Run();
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
