using Serilog;
using Wolverine.Http;
using Zss.BilliardHall.BuildingBlocks.Behaviors;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// 提供 Bootstrapper Host 构建和配置的静态方法，便于测试和复用
/// Provides static methods for building and configuring the Bootstrapper Host, facilitating testing and reuse
/// </summary>
/// <remarks>
/// 此类将 Host 构建逻辑从 Program.cs 中抽离，使得可以在测试中直接构建 Host 而无需 AppHost/Aspire DCP。
/// This class extracts Host building logic from Program.cs, allowing tests to build the Host directly without AppHost/Aspire DCP.
/// </remarks>
public static class BootstrapperHost
{
    /// <summary>
    /// 构建并配置 Bootstrapper 应用程序
    /// Builds and configures the Bootstrapper application
    /// </summary>
    /// <param name="args">命令行参数 / Command line arguments</param>
    /// <returns>配置完成的 WebApplication 实例 / Configured WebApplication instance</returns>
    /// <remarks>
    /// 此方法执行完整的应用程序配置，包括：
    /// - Serilog 日志配置
    /// - ServiceDefaults (OpenTelemetry, HealthChecks, ServiceDiscovery)
    /// - Marten 文档数据库
    /// - Wolverine 命令/消息总线
    /// - 健康检查端点映射
    /// - 根端点
    /// This method performs complete application configuration including:
    /// - Serilog logging configuration
    /// - ServiceDefaults (OpenTelemetry, HealthChecks, ServiceDiscovery)
    /// - Marten document database
    /// - Wolverine command/message bus
    /// - Health check endpoint mapping
    /// - Root endpoint
    /// </remarks>
    public static WebApplication BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        return BuildAppFromBuilder(builder);
    }

    /// <summary>
    /// 从预配置的 builder 构建并配置 Bootstrapper 应用程序
    /// Builds and configures the Bootstrapper application from a pre-configured builder
    /// </summary>
    /// <param name="builder">预配置的 WebApplicationBuilder 实例 / Pre-configured WebApplicationBuilder instance</param>
    /// <returns>配置完成的 WebApplication 实例 / Configured WebApplication instance</returns>
    /// <remarks>
    /// 此方法接受预配置的 builder，允许测试覆盖配置（如连接字符串）。
    /// This method accepts a pre-configured builder, allowing tests to override configuration (like connection strings).
    /// </remarks>
    public static WebApplication BuildAppFromBuilder(WebApplicationBuilder builder)
    {
        ConfigureBuilder(builder);

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.WebHost.UseUrls("http://127.0.0.1:0");
        }

        var app = builder.Build();
        ConfigureApp(app);
        return app;
    }

    /// <summary>
    /// 配置 WebApplicationBuilder，添加所有必需的服务
    /// Configures WebApplicationBuilder by adding all required services
    /// </summary>
    private static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        // Configure Serilog from appsettings.json
        // 从 appsettings.json 配置 Serilog
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        // Add Aspire ServiceDefaults (OpenTelemetry, Health Checks, Service Discovery)
        builder.AddServiceDefaults();

        // Add Marten document database
        builder.AddMartenDefaults();

        // Add Wolverine command/message bus with HTTP endpoints
        builder.AddWolverineDefaults();
        
        // Configure Wolverine to discover handlers in all module assemblies
        // 配置 Wolverine 扫描所有模块程序集中的 Handler
        builder.ConfigureWolverineModuleDiscovery();
    }

    /// <summary>
    /// 配置 WebApplication，映射端点和中间件
    /// Configures WebApplication by mapping endpoints and middleware
    /// </summary>
    private static void ConfigureApp(WebApplication app)
    {
        // Map health check endpoints (/health, /alive)
        app.MapDefaultEndpoints();

        app.UseMiddleware<DomainExceptionMiddleware>();
        app.MapWolverineEndpoints();

        // Map root endpoint with application status
        app.MapGet("/", () => new
        {
            Application = "Zss.BilliardHall.Bootstrapper",
            Status = "Running",
            Framework = "Wolverine + Marten",
            Architecture = "Vertical Slice"
        });
    }
}
