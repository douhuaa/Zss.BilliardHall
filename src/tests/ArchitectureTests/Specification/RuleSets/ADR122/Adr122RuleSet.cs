namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR122;

/// <summary>
/// ADR-122：测试代码组织与命名规范
/// 定义测试类命名、测试项目组织和架构测试分离规则
/// </summary>
public sealed class Adr122RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 122;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(122);

        // Rule 1: 测试类命名规则
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "测试类命名规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "测试类必须以 Tests 结尾",
            enforcement: "验证包含测试方法的类以 Tests 后缀结尾",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "架构测试必须在专用项目中",
            enforcement: "验证存在独立的 ArchitectureTests 项目",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "测试项目必须遵循命名约定",
            enforcement: "验证测试项目命名为 {Module}.Tests 或 ArchitectureTests",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
