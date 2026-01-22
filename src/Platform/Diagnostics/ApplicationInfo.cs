namespace Zss.BilliardHall.Platform.Diagnostics;

/// <summary>
/// 应用程序信息，用于健康检查和诊断
/// </summary>
public sealed class ApplicationInfo
{
    /// <summary>
    /// 应用程序名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 应用程序版本
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// 环境名称（Development, Staging, Production）
    /// </summary>
    public required string Environment { get; init; }

    /// <summary>
    /// 启动时间
    /// </summary>
    public required DateTimeOffset StartTime { get; init; }
}
