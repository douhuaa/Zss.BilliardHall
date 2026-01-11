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
    /// <typeparam name="TBuilder">主机应用程序构建器类型 / The host application builder type.</typeparam>
    /// <param name="builder">主机应用程序构建器实例 / The host application builder instance.</param>
    /// <returns>配置后的构建器实例，支持链式调用 / The configured builder instance for method chaining.</returns>
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
    public static TBuilder AddMartenDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var connectionString = builder.Configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing or empty Default connection string. " +
                "Ensure the database is referenced in AppHost and the connection string is correctly configured and injected.");
        }

        builder.Services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "billiard";
        })
        .UseLightweightSessions(); // Use lightweight sessions by default (recommended for most scenarios)

        return builder;
    }
}
