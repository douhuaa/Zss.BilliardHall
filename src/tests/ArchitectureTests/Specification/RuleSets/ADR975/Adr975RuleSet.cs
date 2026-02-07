namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR975;

/// <summary>
/// ADR-975：监控告警规范
/// 定义治理层相关规则和约束（此为框架性定义，需根据实际测试文件完善）
/// </summary>
public sealed class Adr975RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 975;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(975);

        // Rule 1: 核心规则（需根据测试文件完善）
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "监控告警规范核心规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "待完善：根据对应测试文件补充具体约束条件",
            enforcement: "待完善：根据对应测试文件补充具体执行方式",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
