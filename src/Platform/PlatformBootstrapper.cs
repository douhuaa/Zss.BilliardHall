namespace Zss.BilliardHall.Platform;

public static class PlatformBootstrapper
{
    public static void Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment environment)
    {
        // Logging / Tracing / Error model / Health
        // 示例：Serilog / OpenTelemetry 在这里统一初始化
        // builder.Host.UseSerilog(...);
        // builder.Services.AddOpenTelemetryTracing(...);
        // builder.Services.AddHealthChecks();
    }
}
