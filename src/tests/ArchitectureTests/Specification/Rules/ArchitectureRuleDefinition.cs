namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 架构规则定义
/// 表示一个完整的架构规则（Rule 级别）
/// 
/// 规则是"制度存在"的体现，定义了架构约束的目标和范围
/// </summary>
/// <param name="Id">规则ID（必须是 Rule 级别）</param>
/// <param name="Summary">规则摘要 - 简短描述规则的目的</param>
/// <param name="Severity">严重程度 - 决定违规的处理策略</param>
/// <param name="Scope">作用域 - 决定规则的应用范围</param>
public sealed record ArchitectureRuleDefinition(
    ArchitectureRuleId Id,
    string Summary,
    RuleSeverity Severity,
    RuleScope Scope
)
{
    /// <summary>
    /// 验证规则定义的有效性
    /// </summary>
    public void Validate()
    {
        if (Id.Level != RuleLevel.Rule)
        {
            throw new InvalidOperationException(
                $"ArchitectureRuleDefinition 必须使用 Rule 级别的ID，当前为: {Id}");
        }

        if (string.IsNullOrWhiteSpace(Summary))
        {
            throw new ArgumentException("规则摘要不能为空", nameof(Summary));
        }
    }
}
