namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// Wolverine 模块标记接口
/// </summary>
/// <remarks>
/// 用于显式标识模块边界，支持：
/// 1. 自动模块扫描与注册
/// 2. 权限边界管理
/// 3. Feature Toggle 配置
/// 4. 模块级日志与追踪
/// </remarks>
public interface IWolverineModule
{
    /// <summary>
    /// 模块名称（唯一标识）
    /// </summary>
    static abstract string ModuleName { get; }
}
