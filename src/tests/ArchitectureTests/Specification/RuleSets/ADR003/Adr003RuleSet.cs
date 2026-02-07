using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR003;

/// <summary>
/// ADR-003：命名空间规则
/// 定义命名空间组织、层次结构等规则
/// </summary>
public sealed class Adr003RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 3;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(3);

        // Rule 1: 命名空间层次结构
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "命名空间层次结构",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "根命名空间为 Zss.BilliardHall",
            enforcement: "验证所有类型命名空间以 Zss.BilliardHall 开头",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "模块命名空间为 Zss.BilliardHall.Modules.{ModuleName}",
            enforcement: "验证模块类型命名空间格式",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "Platform 命名空间为 Zss.BilliardHall.Platform",
            enforcement: "验证 Platform 类型命名空间",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: 命名空间与文件夹对应
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "命名空间与文件夹对应",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "命名空间必须与文件夹结构一致",
            enforcement: "验证类型所在文件路径与命名空间匹配",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
