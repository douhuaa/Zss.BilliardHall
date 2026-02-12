using System.Reflection;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Platform;

namespace Zss.BilliardHall.Application;

public static class ApplicationBootstrapper
{
    public static void Configure(
        IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment,
        params Assembly[] moduleAssemblies)
    {
        Configure(services, configuration, environment, enableHttp: false, moduleAssemblies);
    }

    public static void Configure(
        IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment,
        bool enableHttp,
        params Assembly[] moduleAssemblies)
    {
        // 配置 Marten 文档数据库
        ConfigureMarten(services, configuration, environment);
        
        // 配置 Wolverine 消息总线
        ConfigureWolverine(services, enableHttp, moduleAssemblies);
        
        // 加载业务模块
        ModuleLoader.LoadModules(services, configuration, environment, moduleAssemblies);
    }

    private static void ConfigureMarten(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Postgres") 
            ?? "Host=localhost;Port=5432;Database=zss_billiard_hall;Username=postgres;Password=postgres";

        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "public";
        })
        .UseLightweightSessions();
    }

    private static void ConfigureWolverine(IServiceCollection services, bool enableHttp, Assembly[] moduleAssemblies)
    {
        services.AddWolverine(opts =>
        {
            // 发现模块中的 Handler
            foreach (var assembly in moduleAssemblies)
            {
                opts.Discovery.IncludeAssembly(assembly);
            }
            
            // 启用事务
            opts.Policies.AutoApplyTransactions();
        });
        
        // 添加 Wolverine HTTP 支持（仅在 Web Host 中）
        if (enableHttp)
        {
            services.AddWolverineHttp();
        }
    }
}

