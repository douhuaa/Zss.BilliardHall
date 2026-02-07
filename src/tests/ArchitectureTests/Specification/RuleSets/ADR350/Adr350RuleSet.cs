namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR350;

/// <summary>
/// ADR-350：日志与可观测性标签与字段标准
/// 定义日志类型命名空间、框架引用和敏感信息保护规则
/// </summary>
public sealed class Adr350RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 350;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(350);

        // Rule 1: 日志类型组织规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "日志类型组织规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "日志相关类型必须在正确的命名空间",
            enforcement: "验证日志类型位于 Zss.BilliardHall.*.Logging 命名空间",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "项目应引用日志框架",
            enforcement: "验证项目引用 Microsoft.Extensions.Logging 包",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "敏感信息类型不应出现在公共日志中",
            enforcement: "验证不记录 Password、Secret、Token 等敏感类型",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
