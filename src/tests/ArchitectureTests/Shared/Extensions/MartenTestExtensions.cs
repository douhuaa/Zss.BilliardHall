using Marten;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Extensions;

/// <summary>
/// Marten 测试扩展方法
/// 提供测试场景中常用的数据清理和 Schema 管理功能
/// </summary>
public static class MartenTestExtensions
{
    /// <summary>
    /// 清空所有文档数据（保留 Schema 结构）
    /// </summary>
    public static async Task ClearAllDataAsync(this IDocumentStore store, CancellationToken cancellationToken = default)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        try
        {
            await store.Advanced.Clean.DeleteAllDocumentsAsync(cancellationToken);
            Console.WriteLine($"[MartenTestExtensions] 已清空 schema '{store.Options.DatabaseSchemaName}' 的所有文档数据");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MartenTestExtensions] 清空数据时发生错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 完全移除 Schema 中的所有对象（表、函数等）
    /// </summary>
    public static async Task CompletelyRemoveSchemaAsync(this IDocumentStore store, CancellationToken cancellationToken = default)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        try
        {
            // 使用 DeleteAllDocuments 清空数据，不删除 schema 结构
            await store.Advanced.Clean.DeleteAllDocumentsAsync(cancellationToken);
            Console.WriteLine($"[MartenTestExtensions] 已清空 schema '{store.Options.DatabaseSchemaName}' 的所有数据");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MartenTestExtensions] 清空 Schema 时发生错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 重置 Schema：清空所有数据
    /// </summary>
    public static async Task ResetSchemaAsync(this IDocumentStore store, CancellationToken cancellationToken = default)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        try
        {
            // 清空所有数据
            await store.ClearAllDataAsync(cancellationToken);
            
            Console.WriteLine($"[MartenTestExtensions] 已重置 schema '{store.Options.DatabaseSchemaName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MartenTestExtensions] 重置 Schema 时发生错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 清空指定文档类型的所有数据
    /// </summary>
    public static async Task ClearDocumentTypeAsync<T>(this IDocumentStore store, CancellationToken cancellationToken = default) where T : class
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        try
        {
            await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(T), cancellationToken);
            Console.WriteLine($"[MartenTestExtensions] 已清空文档类型 '{typeof(T).Name}' 的所有数据");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MartenTestExtensions] 清空文档类型时发生错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 批量插入测试数据
    /// </summary>
    public static async Task BulkInsertAsync<T>(this IDocumentStore store, IEnumerable<T> documents, CancellationToken cancellationToken = default) where T : class
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        var documentList = documents.ToList();
        if (documentList.Count == 0)
            return;

        await using var session = store.LightweightSession();
        session.Store(documentList.ToArray());
        await session.SaveChangesAsync(cancellationToken);
        
        Console.WriteLine($"[MartenTestExtensions] 已插入 {documentList.Count} 条 '{typeof(T).Name}' 文档");
    }

    /// <summary>
    /// 验证 DocumentStore 连接是否正常
    /// </summary>
    public static async Task<bool> VerifyConnectionAsync(this IDocumentStore store, CancellationToken cancellationToken = default)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        try
        {
            await using var session = store.QuerySession();
            // 简单查询验证连接
            await session.Query<object>().Take(0).ToListAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MartenTestExtensions] 连接验证失败: {ex.Message}");
            return false;
        }
    }
}
