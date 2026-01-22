using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zss.BilliardHall.Platform;

public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // Logging / Tracing / Error model / Health
        // 示例：Serilog / OpenTelemetry 在这里统一初始化
        // builder.Host.UseSerilog(...);
        // builder.Services.AddOpenTelemetryTracing(...);
        // builder.Services.AddHealthChecks();
    }

    /// <summary>
    /// 配置平台层中间件管道
    /// </summary>
    public static void ConfigureMiddleware(
        IApplicationBuilder app,
        IHostEnvironment environment
    )
    {
        // 基础技术中间件
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 健康检查端点
        // app.UseHealthChecks("/health");
        
        // 追踪和监控
        // app.UseOpenTelemetryPrometheusScrapingEndpoint();
    }
}
