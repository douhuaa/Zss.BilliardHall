using Npgsql;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// Wolverine message pipeline 中间件：拦截包含 PostgresException 的异常并转换为 DomainException
/// </summary>
/// <remarks>
/// ⚠️ 这是 Wolverine pipeline 中间件（不是 ASP.NET Core 中间件），
/// 只作用于 Wolverine message handler 的执行链，不影响其他 HTTP 请求处理。
/// 从异常链中提取真正的 PostgresException，遍历已注册的转换器。
/// 命中则抛出对应 DomainException（携带原始 PG 异常作为 InnerException），
/// 未命中则重新抛出原始异常。
/// 注册方式：w.Policies.AddMiddleware&lt;ExceptionTransformMiddleware&gt;()
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
