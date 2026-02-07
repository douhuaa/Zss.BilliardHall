namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR360;

/// <summary>
/// ADR-360：CI/CD Pipeline 流程标准化
/// 定义 GitHub Workflows 配置、PR 模板和架构测试集成规则
/// </summary>
public sealed class Adr360RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 360;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(360);

        // Rule 1: CI/CD 配置规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "CI/CD 配置规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "GitHub Workflows 配置文件应存在",
            enforcement: "验证 .github/workflows 目录存在并包含至少一个 workflow 文件",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "PR 模板应存在",
            enforcement: "验证 .github/PULL_REQUEST_TEMPLATE.md 文件存在",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "架构测试项目应存在并可被 CI 执行",
            enforcement: "验证 ArchitectureTests 项目存在且可被测试运行器发现",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
