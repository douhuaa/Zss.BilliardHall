using Serilog;
using Zss.BilliardHall.Wolverine.Bootstrapper;

// Create a bootstrap logger for early startup logging
// 创建引导日志记录器用于早期启动日志
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Billiard Hall application...");

    var app = HttpHost.Build(args);

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
    Log.CloseAndFlush();
}
