namespace Zss.BilliardHall.Platform.Contracts;

/// <summary>
/// 命令处理器契约：实现 CQRS 模式中的命令处理逻辑
/// </summary>
/// <typeparam name="TCommand">命令类型，通常为 record</typeparam>
/// <typeparam name="TResult">处理结果类型</typeparam>
/// <remarks>
/// 命令处理器负责：
/// - 验证命令
/// - 执行业务逻辑
/// - 持久化状态变更
/// - 返回处理结果
/// Wolverine 会自动发现并注册实现此接口的处理器。
/// </remarks>
public interface ICommandHandler<in TCommand, TResult>
{
    /// <summary>
    /// 处理命令
    /// </summary>
    /// <param name="command">要处理的命令</param>
    /// <returns>处理结果</returns>
    Task<TResult> Handle(TCommand command);
}
