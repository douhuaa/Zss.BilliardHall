namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 健康检查响应
/// 用于 /health 端点返回系统健康状态
/// </summary>
public sealed record HealthCheckResponse
{
    /// <summary>
    /// 健康状态（例如："healthy", "degraded", "unhealthy"）
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// 检查时间戳（UTC）
    /// </summary>
    public DateTime Timestamp { get; init; }
}
