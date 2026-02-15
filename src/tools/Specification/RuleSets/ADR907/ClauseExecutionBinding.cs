namespace Zss.BilliardHall.Specification.RuleSets.ADR907;

/// <summary>
/// 条款执行绑定
/// 定义一个条款如何绑定到具体的执行处理器
/// 
/// 这是规范与执行分离的关键：
/// - ClauseSpec 定义规则的"是什么"（声明式）
/// - ClauseExecutionBinding 定义"如何执行"（绑定到具体处理器）
/// - HandlerKey 是执行处理器的标识符，如 "Analyzer.Enforce.ArchitectureTestPresence"
/// 
/// 未来可扩展为支持多种执行器类型（Analyzer、Runtime、Custom 等）
/// </summary>
/// <param name="RuleId">规则编号</param>
/// <param name="ClauseId">条款编号</param>
/// <param name="HandlerKey">处理器键（标识具体的执行处理器）</param>
public sealed record ClauseExecutionBinding(
    int RuleId,
    int ClauseId,
    string HandlerKey
)
{
    /// <summary>
    /// 验证执行绑定的有效性
    /// </summary>
    public void Validate()
    {
        if (RuleId <= 0)
        {
            throw new ArgumentException($"RuleId 必须大于 0，当前值: {RuleId}", nameof(RuleId));
        }

        if (ClauseId <= 0)
        {
            throw new ArgumentException($"ClauseId 必须大于 0，当前值: {ClauseId}", nameof(ClauseId));
        }

        if (string.IsNullOrWhiteSpace(HandlerKey))
        {
            throw new ArgumentException("HandlerKey 不能为空", nameof(HandlerKey));
        }
    }

    /// <summary>
    /// 获取绑定的完整标识符（用于调试和日志）
    /// </summary>
    public string GetBindingId() => $"({RuleId},{ClauseId}) -> {HandlerKey}";
}
