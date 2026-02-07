namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 架构条款定义
/// 表示规则的具体执行条件（Clause 级别）
/// 
/// 条款是"执行条件"的体现，定义了如何验证和执行规则
/// </summary>
/// <param name="Id">条款ID（必须是 Clause 级别）</param>
/// <param name="Condition">条件描述 - 什么情况下触发此条款</param>
/// <param name="Enforcement">执行要求 - 如何验证和执行此条款</param>
/// <param name="ExecutionType">执行类型 - 定义条款的验证方式（静态分析/约定检查/运行时等）</param>
public sealed record ArchitectureClauseDefinition(
    ArchitectureRuleId Id,
    string Condition,
    string Enforcement,
    ClauseExecutionType ExecutionType
)
{
    /// <summary>
    /// 验证条款定义的有效性
    /// </summary>
    public void Validate()
    {
        if (Id.Level != RuleLevel.Clause)
        {
            throw new InvalidOperationException(
                $"ArchitectureClauseDefinition 必须使用 Clause 级别的ID，当前为: {Id}");
        }

        if (string.IsNullOrWhiteSpace(Condition))
        {
            throw new ArgumentException("条件描述不能为空", nameof(Condition));
        }

        if (string.IsNullOrWhiteSpace(Enforcement))
        {
            throw new ArgumentException("执行要求不能为空", nameof(Enforcement));
        }
    }
}
