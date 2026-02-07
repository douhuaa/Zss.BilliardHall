namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR901;

/// <summary>
/// ADR-901：ADR 风险表达三态语义模型
/// 定义治理层相关规则和约束（此为框架性定义，需根据实际测试文件完善）
/// </summary>
public sealed class Adr901RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 901;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(901);

        // Rule 1: 核心规则（需根据测试文件完善）
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "ADR 风险表达三态语义模型核心规则",
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
