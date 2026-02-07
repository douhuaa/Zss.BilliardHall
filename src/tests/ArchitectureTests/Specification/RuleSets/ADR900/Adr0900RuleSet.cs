namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR900;

/// <summary>
/// ADR-900：架构测试与 CI 治理元规则
/// 定义架构测试的权威性、执行级别、CI 阻断等规则
/// </summary>
public static class Adr0900RuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public const int AdrNumber = 900;

    /// <summary>
    /// 获取完整的规则集定义
    /// </summary>
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);

        // Rule 1: 架构裁决权威性
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "架构裁决权威性",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "ADR 正文是唯一裁决依据",
            enforcement: "验证 ADR 文档存在且包含唯一裁决源声明");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "架构违规的判定原则",
            enforcement: "测试失败、CI Gate 失败、人工否决或破例过期均构成违规");

        // Rule 2: 执行级别与测试映射
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "执行级别与测试映射",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "执行级别分离原则",
            enforcement: "所有规则必须归类为 L1/L2/L3");

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "ADR ↔ 测试 ↔ CI 的一一映射",
            enforcement: "每个 L1 规则必须有对应的架构测试");

        // Rule 3: 破例治理机制
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "破例治理机制",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "破例强制要求",
            enforcement: "所有破例必须通过 Issue 记录并设置到期时间");

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "CI 自动监控机制",
            enforcement: "CI 检查破例是否过期");

        // Rule 4: 冲突裁决优先级
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "冲突裁决优先级",
            severity: RuleSeverity.Governance,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "裁决优先级顺序",
            enforcement: "宪法层 > 治理层 > 技术层，新 ADR > 旧 ADR");

        return ruleSet;
    });
}
