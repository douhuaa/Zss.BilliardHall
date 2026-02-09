namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests.Infrastructure;

/// <summary>
/// RuleSet 验证器
/// 提供 RuleSet 结构完整性和一致性验证
/// 
/// 设计原则：
/// - 单一职责：专注于 RuleSet 的结构验证
/// - 组合优于继承：提供静态方法而非基类
/// - 明确的验证规则：每个验证方法都清楚地说明验证的内容
/// </summary>
public static class RuleSetValidator
{
    /// <summary>
    /// 验证 Rule 的结构完整性
    /// </summary>
    /// <param name="ruleSet">要验证的规则集</param>
    /// <param name="expectedAdrNumber">预期的 ADR 编号</param>
    public static void ValidateRuleStructure(
        ArchitectureRuleSet ruleSet, 
        int expectedAdrNumber)
    {
        ruleSet.AdrNumber.Should().Be(expectedAdrNumber, 
            $"RuleSet 的 ADR 编号应为 {expectedAdrNumber}");

        ruleSet.RuleCount.Should().BeGreaterThan(0, 
            $"ADR-{expectedAdrNumber:000} 必须包含至少一个规则");

        ruleSet.RuleCount.Should().Be(ruleSet.Rules.Count, 
            "RuleCount 应与 Rules 集合的实际数量一致");

        foreach (var rule in ruleSet.Rules)
        {
            ValidateSingleRule(rule, expectedAdrNumber);
        }
    }

    /// <summary>
    /// 验证单个 Rule 的有效性
    /// </summary>
    private static void ValidateSingleRule(
        ArchitectureRuleDefinition rule, 
        int expectedAdrNumber)
    {
        var ruleId = rule.Id.ToString();

        rule.Id.Level.Should().Be(RuleLevel.Rule, 
            $"规则 {ruleId} 的级别应为 Rule");

        rule.Id.AdrNumber.Should().Be(expectedAdrNumber, 
            $"规则 {ruleId} 的 ADR 编号应为 {expectedAdrNumber}");

        rule.Summary.Should().NotBeNullOrWhiteSpace(
            $"规则 {ruleId} 的摘要不应为空");

        // 验证枚举值有效
        Enum.IsDefined(typeof(DecisionLevel), rule.Decision).Should().BeTrue(
            $"规则 {ruleId} 的 DecisionLevel 应为有效枚举值");

        Enum.IsDefined(typeof(RuleSeverity), rule.Severity).Should().BeTrue(
            $"规则 {ruleId} 的 RuleSeverity 应为有效枚举值");

        Enum.IsDefined(typeof(RuleScope), rule.Scope).Should().BeTrue(
            $"规则 {ruleId} 的 RuleScope 应为有效枚举值");
    }

    /// <summary>
    /// 验证 Clause 的结构完整性
    /// </summary>
    /// <param name="ruleSet">要验证的规则集</param>
    /// <param name="expectedAdrNumber">预期的 ADR 编号</param>
    public static void ValidateClauseStructure(
        ArchitectureRuleSet ruleSet, 
        int expectedAdrNumber)
    {
        ruleSet.ClauseCount.Should().BeGreaterThanOrEqualTo(ruleSet.RuleCount,
            $"ADR-{expectedAdrNumber:000} 的条款数应至少等于规则数");

        ruleSet.ClauseCount.Should().Be(ruleSet.Clauses.Count,
            "ClauseCount 应与 Clauses 集合的实际数量一致");

        foreach (var clause in ruleSet.Clauses)
        {
            ValidateSingleClause(clause, expectedAdrNumber);
        }
    }

    /// <summary>
    /// 验证单个 Clause 的有效性
    /// </summary>
    private static void ValidateSingleClause(
        ArchitectureClauseDefinition clause, 
        int expectedAdrNumber)
    {
        var clauseId = clause.Id.ToString();

        clause.Id.Level.Should().Be(RuleLevel.Clause,
            $"条款 {clauseId} 的级别应为 Clause");

        clause.Id.AdrNumber.Should().Be(expectedAdrNumber,
            $"条款 {clauseId} 的 ADR 编号应为 {expectedAdrNumber}");

        clause.Condition.Should().NotBeNullOrWhiteSpace(
            $"条款 {clauseId} 的 Condition 不应为空");

        clause.Enforcement.Should().NotBeNullOrWhiteSpace(
            $"条款 {clauseId} 的 Enforcement 不应为空");

        Enum.IsDefined(typeof(ClauseExecutionType), clause.ExecutionType).Should().BeTrue(
            $"条款 {clauseId} 的 ExecutionType 应为有效枚举值");
    }

    /// <summary>
    /// 验证 Clause 与父 Rule 的关联关系
    /// </summary>
    /// <param name="ruleSet">要验证的规则集</param>
    public static void ValidateClauseToRuleBinding(ArchitectureRuleSet ruleSet)
    {
        foreach (var clause in ruleSet.Clauses)
        {
            var clauseId = clause.Id.ToString();
            var parentRule = ruleSet.GetRule(clause.Id.RuleNumber);

            parentRule.Should().NotBeNull(
                $"条款 {clauseId} 必须有对应的父规则");

            clause.Id.RuleNumber.Should().Be(parentRule!.Id.RuleNumber,
                $"条款 {clauseId} 的 RuleNumber 必须与其父规则匹配");

            clause.Id.AdrNumber.Should().Be(parentRule.Id.AdrNumber,
                $"条款 {clauseId} 的 AdrNumber 必须与其父规则匹配");
        }
    }

    /// <summary>
    /// 验证 RuleSet 的完整性（每个 Rule 至少有一个 Clause）
    /// </summary>
    /// <param name="ruleSet">要验证的规则集</param>
    public static void ValidateCompleteness(ArchitectureRuleSet ruleSet)
    {
        foreach (var rule in ruleSet.Rules)
        {
            var ruleClauses = ruleSet.Clauses
                .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber)
                .ToList();

            ruleClauses.Should().NotBeEmpty(
                $"规则 {rule.Id} 必须至少有一个条款");
        }
    }

    /// <summary>
    /// 执行完整的 RuleSet 验证（组合所有验证）
    /// </summary>
    /// <param name="ruleSet">要验证的规则集</param>
    /// <param name="expectedAdrNumber">预期的 ADR 编号</param>
    public static void ValidateFull(
        ArchitectureRuleSet ruleSet, 
        int expectedAdrNumber)
    {
        ValidateRuleStructure(ruleSet, expectedAdrNumber);
        ValidateClauseStructure(ruleSet, expectedAdrNumber);
        ValidateClauseToRuleBinding(ruleSet);
        ValidateCompleteness(ruleSet);
    }
}
