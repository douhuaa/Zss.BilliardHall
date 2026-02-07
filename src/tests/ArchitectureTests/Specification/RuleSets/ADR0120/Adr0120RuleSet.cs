namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0120;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// ADR-120：领域事件命名规范
/// 定义事件命名、命名空间、内容约束等规则
/// </summary>
public static class Adr0120RuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public const int AdrNumber = 120;

    /// <summary>
    /// 获取完整的规则集定义
    /// </summary>
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);

        // Rule 1: 事件类型命名规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "事件类型命名规范",
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "事件命名模式强制要求",
            enforcement: "验证事件类名以 Event 后缀结尾且使用动词过去式");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "事件命名空间组织规范",
            enforcement: "验证事件在 Modules.{ModuleName}.Events 命名空间下");

        // Rule 2: 事件处理器命名规范
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "事件处理器命名规范",
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "事件处理器命名模式",
            enforcement: "验证处理器以 Handler 后缀结尾");

        // Rule 3: 事件内容约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "事件内容约束",
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "事件内容类型约束",
            enforcement: "验证事件不包含领域实体和业务方法");

        return ruleSet;
    });
}
