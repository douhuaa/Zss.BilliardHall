namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 时间抽象接口
/// </summary>
public interface IClock
{
    /// <summary>
    /// 获取当前 UTC 时间
    /// </summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// 系统时钟实现
/// </summary>
public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
