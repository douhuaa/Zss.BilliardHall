namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 测试基类（分部类）
/// 提供所有规则测试共享的初始化逻辑、辅助方法和公共上下文
/// </summary>
/// <remarks>
/// 设计说明：
/// - 使用 partial class 将不同规则的测试分离到独立文件
/// - 本文件包含公共基础设施（初始化、辅助方法）
/// - 每个规则的测试在独立的分部文件中（如 Adr907Tests.Rule1.cs）
/// - 遵循最佳实践：小方法、单一职责、Theory + InlineData
/// </remarks>
public partial class Adr907Tests
{
    #region 共享字段和属性

    /// <summary>
    /// ADR-907 规则集实例
    /// </summary>
    private readonly Adr907RuleSet _ruleSet;

    /// <summary>
    /// 已定义的架构规则集
    /// </summary>
    private ArchitectureRuleSet RuleSet { get; }

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 ADR-907 测试
    /// </summary>
    public Adr907Tests()
    {
        _ruleSet = new Adr907RuleSet();
        RuleSet = _ruleSet.Define();
    }

    #endregion

    #region 公共辅助方法

    /// <summary>
    /// 获取指定规则
    /// </summary>
    /// <param name="ruleNumber">规则编号</param>
    /// <returns>架构规则，如果不存在则返回 null</returns>
    protected ArchitectureRuleDefinition? GetRule(int ruleNumber)
    {
        return RuleSet.GetRule(ruleNumber);
    }

    /// <summary>
    /// 获取指定条款
    /// </summary>
    /// <param name="ruleNumber">规则编号</param>
    /// <param name="clauseNumber">条款编号</param>
    /// <returns>架构条款，如果不存在则返回 null</returns>
    protected ArchitectureClauseDefinition? GetClause(int ruleNumber, int clauseNumber)
    {
        return RuleSet.GetClause(ruleNumber, clauseNumber);
    }

    /// <summary>
    /// 断言规则存在
    /// </summary>
    /// <param name="ruleNumber">规则编号</param>
    /// <param name="expectedSummary">期望的规则摘要（可选）</param>
    protected void AssertRuleExists(int ruleNumber, string? expectedSummary = null)
    {
        var rule = GetRule(ruleNumber);
        rule.Should().NotBeNull($"Rule {ruleNumber} 必须存在");

        if (!string.IsNullOrEmpty(expectedSummary))
        {
            rule!.Summary.Should().Be(expectedSummary);
        }
    }

    /// <summary>
    /// 断言条款存在
    /// </summary>
    /// <param name="ruleNumber">规则编号</param>
    /// <param name="clauseNumber">条款编号</param>
    /// <param name="expectedCondition">期望的条款条件（可选）</param>
    protected void AssertClauseExists(int ruleNumber, int clauseNumber, string? expectedCondition = null)
    {
        var clause = GetClause(ruleNumber, clauseNumber);
        clause.Should().NotBeNull($"Clause {ruleNumber}.{clauseNumber} 必须存在");

        if (!string.IsNullOrEmpty(expectedCondition))
        {
            clause!.Condition.Should().Contain(expectedCondition);
        }
    }

    /// <summary>
    /// 断言没有违规
    /// 如果有违规，生成详细的错误消息
    /// </summary>
    /// <param name="ruleId">规则 ID（格式：ADR-907_X_Y）</param>
    /// <param name="summary">规则摘要</param>
    /// <param name="violations">违规列表</param>
    /// <param name="remediationSteps">修复步骤</param>
    protected void AssertNoViolations(
        string ruleId,
        string summary,
        IEnumerable<string> violations,
        IEnumerable<string> remediationSteps)
    {
        Adr907TestHelpers.AssertNoViolations(
            ruleId,
            summary,
            violations,
            remediationSteps,
            "docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
    }

    #endregion

    #region 测试辅助类型（仅用于测试）

    /// <summary>
    /// 测试用的占位类型
    /// 避免与项目实际类型冲突
    /// </summary>
    protected internal class TestPlaceholder
    {
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
