using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Builders;

/// <summary>
/// ArchitectureRuleSet 构建器
/// 用于在测试中快速创建架构规则集
/// </summary>
public class ArchitectureRuleSetBuilder : TestDataBuilder<ArchitectureRuleSet, ArchitectureRuleSetBuilder>
{
    private readonly int _adrNumber;

    public ArchitectureRuleSetBuilder(int adrNumber)
    {
        _adrNumber = adrNumber;
        Entity = CreateDefault();
    }

    protected override ArchitectureRuleSet CreateDefault()
    {
        return new ArchitectureRuleSet(_adrNumber);
    }

    /// <summary>
    /// 添加一个规则
    /// </summary>
    public ArchitectureRuleSetBuilder WithRule(
        int ruleNumber,
        string? summary = null,
        DecisionLevel decision = DecisionLevel.Must,
        RuleSeverity severity = RuleSeverity.Governance,
        RuleScope scope = RuleScope.Test)
    {
        Entity.AddRule(
            ruleNumber,
            summary ?? $"规则 {ruleNumber}",
            decision,
            severity,
            scope);
        return This;
    }

    /// <summary>
    /// 添加一个条款
    /// </summary>
    public ArchitectureRuleSetBuilder WithClause(
        int ruleNumber,
        int clauseNumber,
        string? condition = null,
        string? enforcement = null,
        ClauseExecutionType executionType = ClauseExecutionType.Convention)
    {
        Entity.AddClause(
            ruleNumber,
            clauseNumber,
            condition ?? $"条件 {ruleNumber}.{clauseNumber}",
            enforcement ?? $"执行 {ruleNumber}.{clauseNumber}",
            executionType);
        return This;
    }

    /// <summary>
    /// 添加一个完整的规则（Rule + Clause）
    /// </summary>
    public ArchitectureRuleSetBuilder WithCompleteRule(
        int ruleNumber,
        string? summary = null,
        DecisionLevel decision = DecisionLevel.Must,
        RuleSeverity severity = RuleSeverity.Governance,
        RuleScope scope = RuleScope.Test)
    {
        WithRule(ruleNumber, summary, decision, severity, scope);
        WithClause(ruleNumber, 1); // 至少添加一个条款
        return This;
    }

    /// <summary>
    /// 快速添加多个简单规则
    /// </summary>
    public ArchitectureRuleSetBuilder WithRules(params int[] ruleNumbers)
    {
        foreach (var ruleNumber in ruleNumbers)
        {
            WithCompleteRule(ruleNumber);
        }
        return This;
    }
}
