using Marten;

namespace Zss.BilliardHall.Platform.Contracts;

/// <summary>
/// Marten 模块契约：模块可选实现此接口以配置 Marten 文档映射
/// </summary>
/// <remarks>
/// 实现此接口的模块可以：
/// - 配置文档映射和索引
/// - 定义事件流
/// - 自定义序列化行为
/// </remarks>
public interface IMartenModule
{
    /// <summary>
    /// 配置 Marten 存储选项
    /// </summary>
    /// <param name="options">Marten 存储选项</param>
    void ConfigureMarten(StoreOptions options);
}
