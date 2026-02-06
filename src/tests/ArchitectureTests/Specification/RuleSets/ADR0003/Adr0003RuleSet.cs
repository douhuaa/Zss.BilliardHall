namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0003;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// ADR-003：命名空间规则
/// 定义命名空间组织、层次结构等规则
/// </summary>
public static class Adr0003RuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public const int AdrNumber = 3;

    /// <summary>
    /// 获取完整的规则集定义
    /// </summary>
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);

        // Rule 1: 命名空间层次结构
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "命名空间层次结构",
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "根命名空间为 Zss.BilliardHall",
            enforcement: "验证所有类型命名空间以 Zss.BilliardHall 开头");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "模块命名空间为 Zss.BilliardHall.Modules.{ModuleName}",
            enforcement: "验证模块类型命名空间格式");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "Platform 命名空间为 Zss.BilliardHall.Platform",
            enforcement: "验证 Platform 类型命名空间");

        // Rule 2: 命名空间与文件夹对应
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "命名空间与文件夹对应",
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "命名空间必须与文件夹结构一致",
            enforcement: "验证类型所在文件路径与命名空间匹配");

        return ruleSet;
    });
}
