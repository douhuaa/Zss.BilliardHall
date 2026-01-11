using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// 提供 Marten 文档数据库的默认配置扩展方法。
/// Provides extension methods for configuring Marten document database with default settings.
/// </summary>
public static class MartenExtensions
{
    /// <summary>
    /// 添加 Marten 文档数据库的默认配置，包括连接字符串和 schema 命名约定。
    /// Adds Marten document database with default configuration including connection string and schema naming convention.
    /// </summary>
    /// <param name="services">服务集合 / The service collection.</param>
    /// <param name="configuration">配置对象 / The configuration object.</param>
    /// <returns>配置后的服务集合，支持链式调用 / The configured service collection for method chaining.</returns>
    /// <exception cref="InvalidOperationException">当连接字符串 "Default" 未配置时抛出 / Thrown when "Default" connection string is not configured.</exception>
    /// <remarks>
    /// 默认配置：
    /// - 连接字符串键：ConnectionStrings:Default
    /// - Schema 名称：billiard
    /// - 与 AppHost 的 PostgreSQL 资源自动对接
    /// Default configuration:
    /// - Connection string key: ConnectionStrings:Default
    /// - Schema name: billiard
    /// - Automatically connects with AppHost's PostgreSQL resource
    /// </remarks>
    public static IServiceCollection AddMartenDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException(
                                   "Missing Default connection string. " +
                                   "Ensure the database is referenced in AppHost and the connection string is properly injected.");

        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "billiard";
        })
        .UseLightweightSessions(); // Use lightweight sessions by default (recommended for most scenarios)

        return services;
    }
}
