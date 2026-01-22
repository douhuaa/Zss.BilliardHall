using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zss.BilliardHall.Platform.Diagnostics;
using Zss.BilliardHall.Platform.Time;

namespace Zss.BilliardHall.Platform;

public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // 1. 注册时间抽象
        services.AddSingleton<ISystemClock, SystemClock>();

        // 2. 注册应用程序信息
        services.AddSingleton(new ApplicationInfo
        {
            Name = "Zss.BilliardHall",
            Version = "1.0.0",
            Environment = environment.EnvironmentName,
            StartTime = DateTimeOffset.UtcNow
        });

        // 3. 健康检查
        services.AddHealthChecks();

        // 4. 日志和追踪配置将在这里统一初始化
        // 例如：Serilog、OpenTelemetry
        // builder.Host.UseSerilog(...);
        // builder.Services.AddOpenTelemetry()...
    }
}
