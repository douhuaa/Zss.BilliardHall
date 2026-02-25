using Microsoft.AspNetCore.Http;
using Npgsql;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// 异常转换中间件
/// 职责：在 Web 层 GlobalExceptionMiddleware 之前，将技术异常转换为语义异常
/// 
/// 工作流程：
/// 1. 拦截管道中抛出的异常
/// 2. 检查是否为 PostgresException（或包含 PostgresException 的异常链）
/// 3. 调用注册的 IPostgresExceptionTransformer 进行转换
/// 4. 如果转换成功，抛出转换后的领域异常
/// 5. 如果转换失败，重新抛出原始异常
/// 
/// 注意：此中间件应在 GlobalExceptionMiddleware 之前注册
/// </summary>
public sealed class ExceptionTransformMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IPostgresExceptionTransformer> _transformers;

    public ExceptionTransformMiddleware(
        RequestDelegate next,
        IEnumerable<IPostgresExceptionTransformer> transformers)
    {
        _next = next;
        _transformers = transformers;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (ContainsPostgresException(ex))
        {
            // 尝试转换 PostgresException 为领域异常
            foreach (var transformer in _transformers)
            {
                var domainException = transformer.TryTransform(ex);
                if (domainException is not null)
                {
                    throw domainException;
                }
            }

            // 没有转换器匹配，重新抛出原始异常
            throw;
        }
    }

    private static bool ContainsPostgresException(Exception? ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is PostgresException) return true;
        }

        return false;
    }
}
