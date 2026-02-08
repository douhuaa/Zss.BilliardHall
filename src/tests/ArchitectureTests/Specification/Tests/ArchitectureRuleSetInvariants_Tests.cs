using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleSet 不变量清单测试（整合完整版）
/// 验证规则集管理的核心契约，包括创建、添加、完整性和执行类型
/// </summary>
public sealed class ArchitectureRuleSetInvariants_Tests
{
    #region 创建规则集

    [Theory(DisplayName = "不变量：规则集必须指定有效 ADR 编号")]
    [InlineData(1)]
    [InlineData(907)]
    [InlineData(999)]
    public void RuleSet_Should_Have_Valid_AdrNumber(int adrNumber)
    {
        var ruleSet = new ArchitectureRuleSet(adrNumber);
        ruleSet.AdrNumber.Should().Be(adrNumber, "规则集必须记录其 ADR 编号");
    }

    [Theory(DisplayName = "不变量：ADR 编号必须大于0")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void RuleSet_Should_Reject_Invalid_AdrNumber(int invalidAdrNumber)
    {
        Action act = () => new ArchitectureRuleSet(invalidAdrNumber);
        act.Should().Throw<ArgumentException>("ADR 编号必须大于0，保证规则集有效");
    }

    #endregion

    #region 添加规则与条款

    [Fact(DisplayName = "不变量：添加规则后 RuleCount 与 Rules 集合一致")]
    public void Adding_Rule_Should_Increment_RuleCount()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则摘要", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);

        ruleSet.RuleCount.Should().Be(ruleSet.Rules.Count);
        ruleSet.Rules.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "不变量：添加条款后 ClauseCount 与 Clauses 集合一致")]
    public void Adding_Clause_Should_Increment_ClauseCount()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则摘要", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条件", "执行", ClauseExecutionType.Convention);

        ruleSet.ClauseCount.Should().Be(ruleSet.Clauses.Count);
        ruleSet.Clauses.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "不变量：规则摘要不能为空")]
    public void Rule_Summary_Should_Not_Be_Empty()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        Action act = () => ruleSet.AddRule(1, "", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "不变量：条款条件和执行不能为空")]
    public void Clause_Condition_And_Enforcement_Should_Not_Be_Empty()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则摘要", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);

        Action act1 = () => ruleSet.AddClause(1, 1, "", "执行", ClauseExecutionType.Convention);
        Action act2 = () => ruleSet.AddClause(1, 1, "条件", "", ClauseExecutionType.Convention);

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "不变量：不能添加重复规则或条款")]
    public void Should_Not_Allow_Duplicate_Rule_Or_Clause()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则摘要", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条件", "执行", ClauseExecutionType.Convention);

        Action actRule = () => ruleSet.AddRule(1, "重复规则", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        Action actClause = () => ruleSet.AddClause(1, 1, "重复条件", "重复执行", ClauseExecutionType.StaticAnalysis);

        actRule.Should().Throw<InvalidOperationException>();
        actClause.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region 完整性验证

    [Fact(DisplayName = "不变量：ValidateCompleteness 抛异常如果规则没有条款")]
    public void ValidateCompleteness_Should_Throw_When_Rule_Has_No_Clauses()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddRule(2, "规则2-无条款", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);

        Action act = () => ruleSet.ValidateCompleteness();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "不变量：ValidateCompleteness 不抛异常对于完整规则集")]
    public void ValidateCompleteness_Should_Not_Throw_For_Complete_RuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条件1", "执行1", ClauseExecutionType.Convention);

        ruleSet.AddRule(2, "规则2", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(2, 1, "条件2", "执行2", ClauseExecutionType.StaticAnalysis);

        Action act = () => ruleSet.ValidateCompleteness();
        act.Should().NotThrow();
    }

    #endregion

    #region 规则/条款 ID 与 ExecutionType 不变量

    [Fact(DisplayName = "不变量：所有条款 RuleId 必须与父规则一致")]
    public void Clause_RuleId_Should_Match_ParentRule()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则摘要", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条件", "执行", ClauseExecutionType.Convention);

        foreach (var clause in ruleSet.Clauses)
        {
            var parentRule = ruleSet.GetRule(clause.Id.RuleNumber);
            parentRule.Should().NotBeNull("每个条款必须有对应的父规则");
            clause.Id.RuleNumber.Should().Be(parentRule!.Id.RuleNumber, "条款的 RuleNumber 必须与其所属规则匹配");
            clause.Id.AdrNumber.Should().Be(parentRule!.Id.AdrNumber, "条款的 AdrNumber 必须与其所属规则匹配");
        }
    }

    [Fact(DisplayName = "不变量：所有条款 ExecutionType 必须被支持")]
    public void Clause_ExecutionType_Should_Be_Supported()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(1, "规则摘要", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条件", "执行", ClauseExecutionType.Convention);
        ruleSet.AddClause(1, 2, "条件", "执行", ClauseExecutionType.StaticAnalysis);

        foreach (var clause in ruleSet.Clauses)
        {
            ClauseRegistrationStrategyResolver.IsSupported(clause.ExecutionType)
                .Should().BeTrue("每个条款的 ExecutionType 必须被支持");
        }
    }

    #endregion

    #region 完整规则集组合验证（整合 Completeness 场景）

    [Fact(DisplayName = "不变量：完整规则集的 RuleCount 与 ClauseCount 一致")]
    public void Complete_RuleSet_Should_Have_Correct_Counts()
    {
        var ruleSet = new ArchitectureRuleSet(907);

        ruleSet.AddRule(1, "规则1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条件1", "执行1", ClauseExecutionType.Convention);
        ruleSet.AddClause(1, 2, "条件2", "执行2", ClauseExecutionType.StaticAnalysis);

        ruleSet.AddRule(2, "规则2", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(2, 1, "条件3", "执行3", ClauseExecutionType.StaticAnalysis);

        ruleSet.RuleCount.Should().Be(ruleSet.Rules.Count);
        ruleSet.ClauseCount.Should().Be(ruleSet.Clauses.Count);

        foreach (var rule in ruleSet.Rules)
        {
            var ruleClauses = ruleSet.Clauses.Where(c => c.Id.RuleNumber == rule.Id.RuleNumber);
            ruleClauses.Should().NotBeEmpty("完整规则集中的每条规则必须至少有一个条款");
        }

        foreach (var clause in ruleSet.Clauses)
        {
            var parentRule = ruleSet.GetRule(clause.Id.RuleNumber);
            parentRule.Should().NotBeNull();
            clause.Id.RuleNumber.Should().Be(parentRule!.Id.RuleNumber);
            clause.Id.AdrNumber.Should().Be(parentRule!.Id.AdrNumber);
        }
    }

    #endregion
}
