using Zss.BilliardHall;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.File("Logs/logs.txt"))
    .WriteTo.Async(c => c.Console())
    .CreateLogger();

try
{
    Log.Information("启动自助台球系统 API 服务器");
    var builder = WebApplication.CreateBuilder(args);
    
    // 添加 ABP 模块
    builder.Host.AddAppSettingsSecretsJson()
        .UseAutofac()
        .UseSerilog();

    await builder.AddApplicationAsync<BilliardHallHttpApiHostModule>();
    
    var app = builder.Build();
    
    await app.InitializeApplicationAsync();
    
    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    if (ex is HostAbortedException)
    {
        throw;
    }

    Log.Fatal(ex, "服务器启动失败");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}