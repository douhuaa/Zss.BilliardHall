using System.Text;
using Serilog;
using Zss.BilliardHall.Wolverine.Bootstrapper;

// 修复控制台编码为 UTF-8，解决日志中文/其他非 ASCII 字符乱码问题
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Create a bootstrap logger for early startup logging
// 创建引导日志记录器用于早期启动日志
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Billiard Hall application...");

    var app = BootstrapperHost.BuildApp(args);

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
