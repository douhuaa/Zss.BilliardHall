namespace Zss.BilliardHall.Specification.RuleSets.ADR907;

/// <summary>
/// 条款规范
/// 声明式地定义一个条款的元数据，不包含执行逻辑
/// 
/// 这是规范（Spec）和执行（Execution）分离的关键：
/// - ClauseSpec 定义"是什么"（声明式元数据）
/// - ClauseExecutionBinding 定义"如何执行"（执行绑定）
/// - IClauseRegistrationStrategy 实现"怎么注册"（注册策略）
/// </summary>
/// <param name="RuleId">规则编号（1-based）</param>
/// <param name="ClauseId">条款编号（1-based）</param>
/// <param name="Name">条款名称（简短标识）</param>
/// <param name="Description">条款描述（条件描述）</param>
/// <param name="ExecutionType">执行类型</param>
/// <param name="ValidationHint">验证提示（执行要求）</param>
public sealed record ClauseSpec(
    int RuleId,
    int ClauseId,
    string Name,
    string Description,
    ClauseExecutionType ExecutionType,
    string ValidationHint
)
{
    /// <summary>
    /// 验证条款规范的有效性
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

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("Name 不能为空", nameof(Name));
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            throw new ArgumentException("Description 不能为空", nameof(Description));
        }

        if (string.IsNullOrWhiteSpace(ValidationHint))
        {
            throw new ArgumentException("ValidationHint 不能为空", nameof(ValidationHint));
        }
    }

    /// <summary>
    /// 获取条款的完整标识符（格式：ADR-XXX.Rule.Clause）
    /// </summary>
    public string GetFullId(int adrNumber) => $"ADR-{adrNumber:000}.{RuleId}.{ClauseId}";
}
