namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR301;

/// <summary>
/// ADR-301：集成测试环境自动化与隔离约束
/// 定义测试项目组织、命名规范和测试框架标准
/// </summary>
public sealed class Adr301RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 301;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(301);

        // Rule 1: 测试项目组织规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "测试项目组织规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "集成测试项目必须存在",
            enforcement: "验证项目包含至少一个以 Tests 结尾的测试项目",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "TestContainers 配置文件验证",
            enforcement: "验证 src/tests 目录存在并包含测试项目",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "测试项目应使用统一的测试框架",
            enforcement: "验证所有测试项目使用 xUnit 框架",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
