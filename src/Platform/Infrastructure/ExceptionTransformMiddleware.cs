using Npgsql;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// Wolverine message pipeline 中间件：拦截包含 PostgresException 的异常并转换为 DomainException
/// </summary>
/// <remarks>
/// ⚠️ 这是 Wolverine message pipeline 中间件（不是 ASP.NET Core 中间件），
/// 只作用于 Wolverine message handler 的执行链，不影响其他 HTTP 请求处理。
///
/// 工作流程：
/// 1. 通过 <see cref="FindPostgresException"/> 遍历异常链，提取真正的 PostgresException
/// 2. 依次调用所有注册的 <see cref="IPostgresExceptionTransformer"/>，使用第一个匹配结果（短路）
/// 3. 命中则抛出对应 DomainException（携带原始 PG 异常作为 InnerException，保留根因）
/// 4. 未命中或无 PostgresException 则重新抛出原始异常
///
/// 注册方式（ApplicationBootstrapper）：w.Policies.AddMiddleware&lt;ExceptionTransformMiddleware&gt;()
/// 方法签名中的 <see cref="Func{Task}"/> 是 Wolverine codegen 生成的消息处理器调用链延续（continuation），
/// 不同于 ASP.NET Core 的 RequestDelegate 模式。
/// </remarks>
public sealed class ExceptionTransformMiddleware
{
    private readonly IEnumerable<IPostgresExceptionTransformer> _transformers;

    public ExceptionTransformMiddleware(IEnumerable<IPostgresExceptionTransformer> transformers)
    {
        _transformers = transformers;
    }

    public async Task InvokeAsync(Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            var pg = FindPostgresException(ex);
            if (pg is null) throw;

            foreach (var transformer in _transformers)
            {
                var domainException = transformer.TryTransform(pg);
                if (domainException is not null) throw domainException;
            }

            throw;
        }
    }

    private static PostgresException? FindPostgresException(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is PostgresException pg) return pg;
        }

        return null;
    }
}
