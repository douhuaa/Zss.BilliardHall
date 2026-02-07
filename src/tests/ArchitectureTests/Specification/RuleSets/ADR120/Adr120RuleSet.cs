using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR120;

/// <summary>
/// ADR-120：领域事件命名规范
/// 定义事件命名、命名空间、内容约束等规则
/// </summary>
public sealed class Adr120RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 120;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(120);

        // Rule 1: 事件类型命名规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "事件类型命名规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "事件命名模式强制要求",
            enforcement: "验证事件类名以 Event 后缀结尾且使用动词过去式",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "事件命名空间组织规范",
            enforcement: "验证事件在 Modules.{ModuleName}.Events 命名空间下",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: 事件处理器命名规范
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "事件处理器命名规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "事件处理器命名模式",
            enforcement: "验证处理器以 Handler 后缀结尾",
            executionType: ClauseExecutionType.Convention);

        // Rule 3: 事件内容约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "事件内容约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "事件内容类型约束",
            enforcement: "验证事件不包含领域实体和业务方法",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
