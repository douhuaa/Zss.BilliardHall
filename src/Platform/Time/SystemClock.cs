namespace Zss.BilliardHall.Platform.Time;

/// <summary>
/// 系统时钟实现，返回实际的系统时间
/// </summary>
public sealed class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateTimeOffset Now => DateTimeOffset.Now;
}
