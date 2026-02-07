namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907：ArchitectureTests 执法治理体系
/// 定义架构测试的命名、组织、断言等规则
/// </summary>
public static class Adr0907RuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public const int AdrNumber = 907;

    /// <summary>
    /// 获取完整的规则集定义
    /// </summary>
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);

        // Rule 1: 测试文件组织规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "测试文件组织规范",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "测试文件按 ADR 编号组织",
            enforcement: "验证测试文件位于 ADR_{编号} 目录下");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "测试类命名格式 ADR_{编号}_{Rule}_Architecture_Tests",
            enforcement: "验证测试类名称格式");

        // Rule 2: 测试方法命名规范
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "测试方法命名规范",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "测试方法名称包含 RuleId",
            enforcement: "验证方法名以 ADR_{编号}_{Rule}_{Clause}_ 开头");

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "DisplayName 包含完整 RuleId",
            enforcement: "验证 DisplayName 特性包含 ADR-{编号}_{Rule}_{Clause}");

        // Rule 3: 最小断言语义规范
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "最小断言语义规范",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "每个测试类至少包含1个有效断言",
            enforcement: "通过静态分析验证断言数量");

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "每个测试方法只能映射一个ADR子规则",
            enforcement: "通过命名模式检查验证");

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 3,
            condition: "所有断言失败信息必须可反向溯源到ADR",
            enforcement: "验证失败消息包含ADR引用、违规标记、修复建议和文档引用");

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 4,
            condition: "禁止形式化断言",
            enforcement: "禁止 Assert.True(true) 等无意义断言");

        // Rule 4: RuleId 格式规范
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "RuleId 格式规范",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "RuleId 格式为 ADR-{编号}_{Rule}_{Clause}",
            enforcement: "验证所有 RuleId 使用下划线分隔");

        return ruleSet;
    });
}
