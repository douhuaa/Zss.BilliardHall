using System.Text;
using Serilog;
using Zss.BilliardHall.Wolverine.Bootstrapper;

Console.OutputEncoding = Encoding.UTF8;

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
}
finally
{
    Log.CloseAndFlush();
}
