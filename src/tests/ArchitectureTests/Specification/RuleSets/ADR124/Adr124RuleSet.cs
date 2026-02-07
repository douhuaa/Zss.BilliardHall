namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR124;

/// <summary>
/// ADR-124：Endpoint 命名及参数约束规范
/// 定义 Endpoint 类命名和请求 DTO 命名规则
/// </summary>
public sealed class Adr124RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 124;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(124);

        // Rule 1: Endpoint 命名规则
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Endpoint 命名规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Endpoint 类必须遵循命名规范",
            enforcement: "验证 Endpoint 类以 Endpoint 后缀结尾",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "请求 DTO 必须以 Request 结尾",
            enforcement: "验证请求数据传输对象以 Request 后缀结尾",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
