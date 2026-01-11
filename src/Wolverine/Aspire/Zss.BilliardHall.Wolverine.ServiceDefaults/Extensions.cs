using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Wolverine;
using Wolverine.FluentValidation;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// 提供 Aspire 服务默认配置的扩展方法，包括服务发现、健康检查、可恢复性和 OpenTelemetry 可观测性。
/// 此项目应被解决方案中的每个服务项目引用。
/// Provides extension methods for configuring Aspire service defaults, including service discovery, health checks, resilience, and OpenTelemetry observability.
/// This project should be referenced by each service project in your solution.
/// </summary>
/// <remarks>
/// 更多信息请参考 / To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
/// </remarks>
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// 添加常用的 Aspire 服务，包括 OpenTelemetry、健康检查、服务发现和 HTTP 客户端可恢复性配置。
    /// Adds common Aspire services including OpenTelemetry, health checks, service discovery, and HTTP client resilience configuration.
    /// </summary>
    /// <typeparam name="TBuilder">主机应用程序构建器类型 / The host application builder type.</typeparam>
    /// <param name="builder">主机应用程序构建器实例 / The host application builder instance.</param>
    /// <returns>配置后的构建器实例，支持链式调用 / The configured builder instance for method chaining.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    /// <summary>
    /// 配置 OpenTelemetry，包括日志、指标和分布式追踪。
    /// Configures OpenTelemetry including logging, metrics, and distributed tracing.
    /// </summary>
    /// <typeparam name="TBuilder">主机应用程序构建器类型 / The host application builder type.</typeparam>
    /// <param name="builder">主机应用程序构建器实例 / The host application builder instance.</param>
    /// <returns>配置后的构建器实例，支持链式调用 / The configured builder instance for method chaining.</returns>
    /// <remarks>
    /// 自动包含 ASP.NET Core、HTTP 客户端和运行时指标/追踪仪器。
    /// 健康检查请求会自动从追踪中排除。
    /// Automatically includes ASP.NET Core, HTTP client, and runtime metrics/tracing instrumentation.
    /// Health check requests are automatically excluded from tracing.
    /// </remarks>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                        // Exclude health check requests from tracing
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    /// <summary>
    /// 添加默认健康检查，包括基本的存活性检查。
    /// Adds default health checks including a basic liveness check.
    /// </summary>
    /// <typeparam name="TBuilder">主机应用程序构建器类型 / The host application builder type.</typeparam>
    /// <param name="builder">主机应用程序构建器实例 / The host application builder instance.</param>
    /// <returns>配置后的构建器实例，支持链式调用 / The configured builder instance for method chaining.</returns>
    /// <remarks>
    /// 默认添加一个标记为 "live" 的自检查，确保应用程序响应正常。
    /// By default adds a self-check tagged with "live" to ensure the application is responsive.
    /// </remarks>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// 映射默认的健康检查端点（仅在开发环境启用，限制为 localhost 访问）。
    /// Maps default health check endpoints (only enabled in development environment, restricted to localhost access).
    /// </summary>
    /// <param name="app">Web 应用程序实例 / The web application instance.</param>
    /// <returns>配置后的 Web 应用程序实例，支持链式调用 / The configured web application instance for method chaining.</returns>
    /// <remarks>
    /// <para>映射两个端点 / Maps two endpoints:</para>
    /// <list type="bullet">
    /// <item><description>/health - 就绪检查，所有健康检查必须通过 / Readiness check, all health checks must pass</description></item>
    /// <item><description>/alive - 存活检查，仅标记为 "live" 的健康检查必须通过 / Liveness check, only "live" tagged checks must pass</description></item>
    /// </list>
    /// <para>
    /// 安全考虑：端点仅限 localhost 访问，避免泄露系统状态、依赖关系和基础设施信息。
    /// Security consideration: Endpoints are restricted to localhost to prevent leaking system state, dependencies, and infrastructure information.
    /// </para>
    /// <para>
    /// 在生产环境启用健康检查端点有安全隐患，详情请参考 / 
    /// Enabling health check endpoints in non-development environments has security implications. See https://aka.ms/dotnet/aspire/healthchecks
    /// </para>
    /// </remarks>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            // Restrict to localhost to reduce information leakage risk
            var readinessEndpoint = app.MapHealthChecks(HealthEndpointPath);
            readinessEndpoint.RequireHost("localhost", "127.0.0.1");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            // Restrict to localhost to reduce information leakage risk
            var livenessEndpoint = app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
            livenessEndpoint.RequireHost("localhost", "127.0.0.1");
        }

        return app;
    }

    /// <summary>
    /// 添加 Wolverine 默认配置，包括命令总线、消息总线和 HTTP 端点。
    /// Adds Wolverine default configuration, including command bus, message bus, and HTTP endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">主机应用程序构建器类型 / The host application builder type.</typeparam>
    /// <param name="builder">主机应用程序构建器实例 / The host application builder instance.</param>
    /// <returns>配置后的构建器实例，支持链式调用 / The configured builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// 此方法配置 Wolverine 以支持垂直切片架构（Vertical Slice Architecture）。
    /// This method configures Wolverine to support Vertical Slice Architecture.
    /// </para>
    /// <para>
    /// 默认配置包括 / Default configuration includes:
    /// </para>
    /// <list type="bullet">
    /// <item><description>HTTP 端点自动发现 / Automatic HTTP endpoint discovery</description></item>
    /// <item><description>FluentValidation 集成 / FluentValidation integration</description></item>
    /// <item><description>为未来的消息队列集成预留配置点 / Configuration point for future message queue integration (RabbitMQ/Kafka)</description></item>
    /// </list>
    /// <para>
    /// 使用示例 / Usage example:
    /// </para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.AddServiceDefaults();
    /// builder.AddWolverineDefaults();
    /// </code>
    /// </remarks>
    public static TBuilder AddWolverineDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddWolverine(opts =>
        {
            // 启用 HTTP 端点自动发现
            // Enable automatic HTTP endpoint discovery
            opts.Discovery.IncludeAssembly(typeof(TBuilder).Assembly);

            // 集成 FluentValidation 进行输入验证
            // Integrate FluentValidation for input validation
            opts.UseFluentValidation();

            // 预留配置点：未来可以在此集成消息队列
            // Configuration placeholder: Future message queue integration
            // 示例 / Examples:
            // - RabbitMQ: opts.UseRabbitMq(...)
            // - Kafka: opts.UseKafka(...)
            // - Azure Service Bus: opts.UseAzureServiceBus(...)

            // 从配置中读取消息队列设置（如果有）
            // Read message queue settings from configuration (if available)
            var messagingSection = builder.Configuration.GetSection("Wolverine:Messaging");
            if (messagingSection.Exists())
            {
                // 未来可以根据配置动态加载传输层
                // Future: Dynamically load transport based on configuration
                // var provider = messagingSection["Provider"]; // "RabbitMQ", "Kafka", etc.
            }
        });

        return builder;
    }
}
