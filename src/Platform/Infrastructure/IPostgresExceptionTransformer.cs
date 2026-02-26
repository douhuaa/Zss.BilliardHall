using Npgsql;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// PostgreSQL 异常转换器契约：将 PostgresException 直接转换为 DomainException
/// </summary>
/// <remarks>
/// 用途：在 Wolverine pipeline 层，将已确认的 PostgresException
/// 转换为 DomainException，避免 Web 层感知具体基础设施类型。
/// 各模块自行注册实现，遵循垂直切片架构。
/// </remarks>
public interface IPostgresExceptionTransformer
{
    /// <summary>
    /// 尝试将 PostgresException 转换为语义更丰富的领域异常
    /// </summary>
    /// <param name="ex">确认命中的 PostgreSQL 异常</param>
    /// <returns>转换后的领域异常；如果不适用则返回 null</returns>
    DomainException? TryTransform(PostgresException ex);
}
