namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR123;

/// <summary>
/// ADR-123：Repository 接口与分层命名规范
/// 定义 Repository 接口位置、命名模式和聚合根对应规则
/// </summary>
public sealed class Adr123RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 123;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(123);

        // Rule 1: Repository 接口分层约束
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Repository 接口分层约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Repository 接口必须位于 Domain 层",
            enforcement: "验证 Repository 接口位于 *.Domain.* 命名空间",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "Repository 接口命名必须遵循 I{Aggregate}Repository 模式",
            enforcement: "验证 Repository 接口以 I 开头、Repository 结尾",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
