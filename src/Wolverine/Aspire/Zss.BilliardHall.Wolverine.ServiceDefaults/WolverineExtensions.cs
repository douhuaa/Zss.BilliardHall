using Microsoft.Extensions.Configuration;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;

namespace Microsoft.Extensions.Hosting;

public static class WolverineExtensions
{
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
    /// <item><description>自动扫描入口程序集的 Handler 和 Endpoint / Automatic discovery of Handlers and Endpoints in the entry assembly</description></item>
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
        builder.Services.AddWolverineHttp();

        builder.Services.AddWolverine(opts =>
        {
            // Wolverine 默认会自动扫描入口程序集（entry assembly）查找 Handler 和 Endpoint
            // Wolverine automatically scans the entry assembly for Handlers and Endpoints by default
            // 如果需要扫描额外的程序集，可以使用：
            // To scan additional assemblies, use:
            // opts.Discovery.IncludeAssembly(typeof(SomeType).Assembly);

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
