using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Zss.BilliardHall.DbMigrator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("启动数据库迁移器");
            
            await CreateHostBuilder(args).RunConsoleAsync();
            
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "数据库迁移器启动失败");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    internal static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<DbMigratorHostedService>();
            });
}