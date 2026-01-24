namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// ADR-240: 标记接口，表示异常可通过重试解决
/// Marker interface indicating that an exception may be resolved through retry
/// </summary>
public interface IRetryable
{
    /// <summary>
    /// 获取建议的重试延迟（毫秒）
    /// Gets the suggested retry delay in milliseconds
    /// </summary>
    int? SuggestedRetryDelayMs { get; }
}
