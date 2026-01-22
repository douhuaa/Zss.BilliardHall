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
        // 1. 注册应用程序信息（在注册时间抽象之前，使用实际系统时间）
        var startTime = DateTimeOffset.UtcNow;
        services.AddSingleton(new ApplicationInfo
        {
            Name = "Zss.BilliardHall",
            Version = "1.0.0",
            Environment = environment.EnvironmentName,
            StartTime = startTime
        });

        // 2. 注册时间抽象（供业务逻辑使用）
        services.AddSingleton<ISystemClock, SystemClock>();

        // 3. 健康检查
        services.AddHealthChecks();

        // 4. 日志和追踪配置将在这里统一初始化
        // 例如：Serilog、OpenTelemetry
        // builder.Host.UseSerilog(...);
        // builder.Services.AddOpenTelemetry()...
    }
}
