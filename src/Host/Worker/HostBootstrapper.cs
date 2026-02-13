using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

namespace Zss.BilliardHall.Host.Worker;

/// <summary>
/// Worker Host 层统一 Bootstrapper
/// 封装所有初始化逻辑，简化 Program.cs
/// </summary>
public static class HostBootstrapper
{
    /// <summary>
    /// 配置服务容器
    /// </summary>
    public static void ConfigureServices(HostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // 1. 配置 Platform 层（基础设施）
        PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

        // 2. Host 层决定加载哪些模块（类型安全）
        var logger = CreateBootstrapLogger(builder);
        var moduleAssemblies = ModuleRegistry.GetEnabledAssemblies(builder.Configuration, logger);

        // 3. 配置 Application 层（业务装配）
        ApplicationBootstrapper.Configure(
            builder.Services,
            builder.Configuration,
            builder.Environment,
            moduleAssemblies);

        logger.LogInformation("Worker Host 服务配置完成");
    }

    /// <summary>
    /// 创建早期日志记录器（用于启动阶段）
    /// </summary>
    private static ILogger CreateBootstrapLogger(HostApplicationBuilder builder)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
        });

        return loggerFactory.CreateLogger("HostBootstrapper");
    }
}

