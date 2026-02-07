namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleSet 的单元测试
/// 验证规则集管理功能
/// </summary>
public sealed class ArchitectureRuleSetTests
{
    private const string AdrFormat = "ADR-{0:000}";

    [Theory(DisplayName = "应该能创建规则集并指定 ADR 编号")]
    [InlineData(907)]
    [InlineData(1)]
    [InlineData(900)]
    public void Should_Create_RuleSet_With_Adr_Number(int adrNumber)
    {
        var ruleSet = CreateRuleSet(adrNumber);

        ruleSet.AdrNumber.Should().Be(adrNumber);
        ruleSet.RuleCount.Should().Be(0);
        ruleSet.ClauseCount.Should().Be(0);
    }

    [Theory(DisplayName = "创建规则集时 ADR 编号必须大于0")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Throw_When_Adr_Number_Is_Zero_Or_Negative(int invalidAdrNumber)
    {
        Action act = () => new ArchitectureRuleSet(invalidAdrNumber);
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "应该能添加规则")]
    public void Should_Add_Rule()
    {
        var ruleSet = CreateRuleSet(907);

        ruleSet.AddRule(3, "最小断言语义规范", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);

        ruleSet.RuleCount.Should().Be(1);
        ruleSet.Rules.Should().HaveCount(1);

        var rule = ruleSet.GetRule(3);
        AssertRuleExists(rule, 907, 3, "最小断言语义规范", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
    }

    [Fact(DisplayName = "应该能添加条款")]
    public void Should_Add_Clause()
    {
        var ruleSet = CreateRuleSet(907);

        ruleSet.AddClause(3, 1, "测试类至少包含1个有效断言", "通过静态分析验证", ClauseExecutionType.StaticAnalysis);

        ruleSet.ClauseCount.Should().Be(1);
        ruleSet.Clauses.Should().HaveCount(1);

        var clause = ruleSet.GetClause(3, 1);
        AssertClauseExists(clause, 907, 3, 1, "测试类至少包含1个有效断言", "通过静态分析验证", ClauseExecutionType.StaticAnalysis);
    }

    [Theory(DisplayName = "不能添加重复的规则")]
    [InlineData(907, 3)]
    [InlineData(1, 1)]
    [InlineData(900, 5)]
    public void Should_Not_Add_Duplicate_Rule(int adr, int ruleNumber)
    {
        var ruleSet = CreateRuleSet(adr);
        ruleSet.AddRule(ruleNumber, "规则1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);

        Action act = () => ruleSet.AddRule(ruleNumber, "规则2", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{BuildRuleKey(adr, ruleNumber)}*已存在*");
    }

    [Theory(DisplayName = "不能添加重复的条款")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 5, 2)]
    public void Should_Not_Add_Duplicate_Clause(int adr, int ruleNumber, int clauseNumber)
    {
        var ruleSet = CreateRuleSet(adr);
        ruleSet.AddClause(ruleNumber, clauseNumber, "条款1", "执行1", ClauseExecutionType.Convention);

        Action act = () => ruleSet.AddClause(ruleNumber, clauseNumber, "条款2", "执行2", ClauseExecutionType.StaticAnalysis);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{BuildClauseKey(adr, ruleNumber, clauseNumber)}*已存在*");
    }

    [Theory(DisplayName = "HasRule 应该正确检查规则是否存在")]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(1, false)]
    public void HasRule_Should_Check_Rule_Existence(int ruleNumber, bool expected)
    {
        var ruleSet = CreateRuleSet(907);
        ruleSet.AddRule(3, "规则", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);

        ruleSet.HasRule(ruleNumber).Should().Be(expected);
    }

    [Theory(DisplayName = "HasClause 应该正确检查条款是否存在")]
    [InlineData(3, 1, true)]
    [InlineData(3, 2, false)]
    [InlineData(4, 1, false)]
    public void HasClause_Should_Check_Clause_Existence(int ruleNumber, int clauseNumber, bool expected)
    {
        var ruleSet = CreateRuleSet(907);
        ruleSet.AddClause(3, 1, "条款", "执行", ClauseExecutionType.Convention);

        ruleSet.HasClause(ruleNumber, clauseNumber).Should().Be(expected);
    }

    [Fact(DisplayName = "应该能构建完整的 ADR 规则集")]
    public void Should_Build_Complete_Adr_RuleSet()
    {
        var ruleSet = CreateRuleSet(907);

        ruleSet.AddRule(3, "最小断言语义规范", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(3, 1, "测试类至少包含1个有效断言", "静态分析", ClauseExecutionType.StaticAnalysis);
        ruleSet.AddClause(3, 2, "测试方法只能映射一个子规则", "命名检查", ClauseExecutionType.Convention);
        ruleSet.AddClause(3, 3, "失败信息必须可溯源", "消息格式验证", ClauseExecutionType.Convention);
        ruleSet.AddClause(3, 4, "禁止形式化断言", "模式匹配检查", ClauseExecutionType.StaticAnalysis);

        ruleSet.RuleCount.Should().Be(1);
        ruleSet.ClauseCount.Should().Be(4);

        var rule = ruleSet.GetRule(3);
        rule.Should().NotBeNull();
        rule!.Id.Level.Should().Be(RuleLevel.Rule);

        for (int i = 1; i <= 4; i++)
        {
            var clause = ruleSet.GetClause(3, i);
            clause.Should().NotBeNull();
            clause!.Id.Level.Should().Be(RuleLevel.Clause);
        }
    }

    [Theory(DisplayName = "不能添加空摘要的规则")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_Not_Add_Rule_With_Empty_Summary(string emptySummary)
    {
        var ruleSet = CreateRuleSet(907);
        Action act = () => ruleSet.AddRule(3, emptySummary, DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        act.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "不能添加空描述的条款")]
    [InlineData("", "执行")]
    [InlineData("   ", "执行")]
    [InlineData("条件", "")]
    [InlineData("条件", "   ")]
    public void Should_Not_Add_Clause_With_Empty_Description(string condition, string enforcement)
    {
        var ruleSet = CreateRuleSet(907);
        Action act = () => ruleSet.AddClause(3, 1, condition, enforcement, ClauseExecutionType.Convention);
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "ValidateCompleteness 应该检测没有条款的规则")]
    public void ValidateCompleteness_Should_Detect_Rules_Without_Clauses()
    {
        var ruleSet = CreateRuleSet(907);
        ruleSet.AddRule(1, "有条款的规则", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条款1", "执行1", ClauseExecutionType.Convention);
        ruleSet.AddRule(2, "没有条款的规则", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);

        Action act = () => ruleSet.ValidateCompleteness();
        act.Should().Throw<InvalidOperationException>().WithMessage($"*{BuildRuleKey(907, 2)}*");
    }

    [Fact(DisplayName = "ValidateCompleteness 应该通过有完整条款的规则集")]
    public void ValidateCompleteness_Should_Pass_Complete_RuleSet()
    {
        var ruleSet = CreateRuleSet(907);
        ruleSet.AddRule(1, "规则1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "条款1", "执行1", ClauseExecutionType.Convention);
        ruleSet.AddRule(2, "规则2", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(2, 1, "条款2", "执行2", ClauseExecutionType.StaticAnalysis);

        Action act = () => ruleSet.ValidateCompleteness();
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "ValidateCompleteness 应该通过空规则集")]
    public void ValidateCompleteness_Should_Pass_Empty_RuleSet()
    {
        var ruleSet = CreateRuleSet(907);
        Action act = () => ruleSet.ValidateCompleteness();
        act.Should().NotThrow();
    }

    #region 辅助方法

    private static ArchitectureRuleSet CreateRuleSet(int adrNumber) =>
        new ArchitectureRuleSet(adrNumber);

    private static void AssertRuleExists(
        ArchitectureRuleDefinition? rule,
        int adr, int ruleNum,
        string summary,
        DecisionLevel decision,
        RuleSeverity severity,
        RuleScope scope)
    {
        rule.Should().NotBeNull();
        rule!.Id.ToString().Should().Be($"{string.Format(AdrFormat, adr)}_{ruleNum}");
        rule.Summary.Should().Be(summary);
        rule.Decision.Should().Be(decision);
        rule.Severity.Should().Be(severity);
        rule.Scope.Should().Be(scope);
    }

    private static void AssertClauseExists(
        ArchitectureClauseDefinition? clause,
        int adr, int ruleNum, int clauseNum,
        string condition,
        string enforcement,
        ClauseExecutionType executionType)
    {
        clause.Should().NotBeNull();
        clause!.Id.ToString().Should().Be($"{string.Format(AdrFormat, adr)}_{ruleNum}_{clauseNum}");
        clause.Condition.Should().Be(condition);
        clause.Enforcement.Should().Be(enforcement);
        clause.ExecutionType.Should().Be(executionType);
    }

    private static string BuildRuleKey(int adr, int ruleNumber) => $"{string.Format(AdrFormat, adr)}_{ruleNumber}";
    private static string BuildClauseKey(int adr, int ruleNumber, int clauseNumber) => $"{string.Format(AdrFormat, adr)}_{ruleNumber}_{clauseNumber}";

    #endregion
}
