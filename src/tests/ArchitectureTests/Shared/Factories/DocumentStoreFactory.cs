using Marten;
using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Factories;

/// <summary>
/// Marten DocumentStore 工厂类
/// 用于创建测试隔离的 DocumentStore 实例
/// </summary>
public static class DocumentStoreFactory
{
    /// <summary>
    /// 创建测试用的 DocumentStore
    /// </summary>
    /// <param name="connectionString">PostgreSQL 连接字符串</param>
    /// <param name="schema">Schema 名称，若为 null 则自动生成唯一 schema_{guid}</param>
    /// <param name="loggerFactory">可选的日志工厂</param>
    /// <param name="configureOptions">可选的额外配置</param>
    /// <returns>配置好的 DocumentStore 实例</returns>
    public static IDocumentStore Create(
        string connectionString,
        string? schema = null,
        ILoggerFactory? loggerFactory = null,
        Action<StoreOptions>? configureOptions = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("连接字符串不能为空", nameof(connectionString));
        }

        // 生成唯一 schema 用于测试隔离
        var schemaName = schema ?? $"test_schema_{Guid.NewGuid():N}";

        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = schemaName;
            
            // 应用额外配置
            configureOptions?.Invoke(options);
        });

        return store;
    }

    /// <summary>
    /// 创建用于测试集合（Collection）级别共享的 DocumentStore
    /// 使用命名 schema 而非随机 guid
    /// </summary>
    /// <param name="connectionString">PostgreSQL 连接字符串</param>
    /// <param name="collectionName">集合名称，用于生成 schema 名</param>
    /// <param name="loggerFactory">可选的日志工厂</param>
    /// <param name="configureOptions">可选的额外配置</param>
    /// <returns>配置好的 DocumentStore 实例</returns>
    public static IDocumentStore CreateForCollection(
        string connectionString,
        string collectionName,
        ILoggerFactory? loggerFactory = null,
        Action<StoreOptions>? configureOptions = null)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("集合名称不能为空", nameof(collectionName));
        }

        // 使用集合名称 + 时间戳生成 schema，便于识别和清理
        var schemaName = $"test_{SanitizeSchemaName(collectionName)}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        
        return Create(connectionString, schemaName, loggerFactory, configureOptions);
    }

    /// <summary>
    /// 清理 schema 名称，移除不合法字符
    /// </summary>
    private static string SanitizeSchemaName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "default";

        // 只保留字母、数字和下划线
        var sanitized = new string(name
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray());

        // 确保以字母开头
        if (sanitized.Length > 0 && !char.IsLetter(sanitized[0]))
        {
            sanitized = "s_" + sanitized;
        }

        return sanitized;
    }
}
