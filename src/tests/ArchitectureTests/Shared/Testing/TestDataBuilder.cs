namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Infrastructure;

/// <summary>
/// 测试数据构建器
/// 提供流式API创建测试所需的 RuleSet、Rule 和 Clause
/// 
/// 设计原则：
/// - Fluent API：提供链式调用的流式接口
/// - 合理默认值：简化常见场景的数据创建
/// - 可定制性：允许覆盖所有字段
/// </summary>
public sealed class TestDataBuilder
{
    /// <summary>
    /// 创建一个新的 RuleSet 构建器
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    public static RuleSetBuilder CreateRuleSet(int adrNumber)
        => new(adrNumber);

    /// <summary>
    /// RuleSet 构建器
    /// </summary>
    public sealed class RuleSetBuilder
    {
        private readonly ArchitectureRuleSet _ruleSet;

        internal RuleSetBuilder(int adrNumber)
        {
            _ruleSet = new ArchitectureRuleSet(adrNumber);
        }

        /// <summary>
        /// 添加一个规则（使用默认值）
        /// </summary>
        public RuleSetBuilder WithRule(
            int ruleNumber,
            string? summary = null,
            DecisionLevel decision = DecisionLevel.Must,
            RuleSeverity severity = RuleSeverity.Governance,
            RuleScope scope = RuleScope.Test)
        {
            _ruleSet.AddRule(
                ruleNumber,
                summary ?? $"规则 {ruleNumber}",
                decision,
                severity,
                scope);
            return this;
        }

        /// <summary>
        /// 添加一个条款（使用默认值）
        /// </summary>
        public RuleSetBuilder WithClause(
            int ruleNumber,
            int clauseNumber,
            string? condition = null,
            string? enforcement = null,
            ClauseExecutionType executionType = ClauseExecutionType.Convention)
        {
            _ruleSet.AddClause(
                ruleNumber,
                clauseNumber,
                condition ?? $"条件 {ruleNumber}.{clauseNumber}",
                enforcement ?? $"执行 {ruleNumber}.{clauseNumber}",
                executionType);
            return this;
        }

        /// <summary>
        /// 添加一个完整的规则（Rule + Clause）
        /// </summary>
        public RuleSetBuilder WithCompleteRule(
            int ruleNumber,
            string? summary = null,
            DecisionLevel decision = DecisionLevel.Must,
            RuleSeverity severity = RuleSeverity.Governance,
            RuleScope scope = RuleScope.Test)
        {
            WithRule(ruleNumber, summary, decision, severity, scope);
            WithClause(ruleNumber, 1); // 至少添加一个条款
            return this;
        }

        /// <summary>
        /// 构建最终的 RuleSet
        /// </summary>
        public ArchitectureRuleSet Build()
        {
            return _ruleSet;
        }
    }
}
