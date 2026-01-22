namespace Zss.BilliardHall.Platform.Time;

/// <summary>
/// 系统时钟抽象，用于获取当前时间
/// 便于测试和时间相关的业务逻辑
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// 获取当前 UTC 时间
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// 获取当前本地时间
    /// </summary>
    DateTimeOffset Now { get; }
}
