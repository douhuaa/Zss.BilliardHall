namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR210;

/// <summary>
/// ADR-210：领域事件版本化与兼容性
/// 定义事件 SchemaVersion 属性要求和版本控制规则
/// </summary>
public sealed class Adr210RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 210;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(210);

        // Rule 2: 事件 SchemaVersion 要求
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "事件 SchemaVersion 要求",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Runtime,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "事件必须包含 SchemaVersion 属性",
            enforcement: "验证所有领域事件包含 SchemaVersion 属性以支持版本化和向后兼容",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
