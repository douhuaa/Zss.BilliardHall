using Microsoft.AspNetCore.Http;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// PostgresException 异常转换器契约
/// 职责：将 PostgresException 转换为领域异常
/// 
/// 设计原则：
/// - 每个模块注册自己的转换器
/// - 转换器在 Wolverine HTTP 管道之外、Web GlobalExceptionMiddleware 之前执行
/// - Web 层不需要感知 Npgsql 类型
/// </summary>
public interface IPostgresExceptionTransformer
{
    /// <summary>
    /// 尝试将异常转换为领域异常
    /// </summary>
    /// <param name="ex">原始异常（通常是 PostgresException 或包含 PostgresException 的异常链）</param>
    /// <returns>转换后的领域异常；如果不适用则返回 null</returns>
    DomainException? TryTransform(Exception ex);
}
