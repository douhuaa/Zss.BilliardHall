namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleSet 的单元测试
/// 验证规则集管理功能
/// </summary>
public sealed class ArchitectureRuleSetTests
{
    [Fact(DisplayName = "应该能创建规则集并指定 ADR 编号")]
    public void Should_Create_RuleSet_With_Adr_Number()
    {
        // Arrange & Act
        var ruleSet = new ArchitectureRuleSet(907);

        // Assert
        ruleSet.AdrNumber.Should().Be(907);
        ruleSet.RuleCount.Should().Be(0);
        ruleSet.ClauseCount.Should().Be(0);
    }

    [Fact(DisplayName = "创建规则集时 ADR 编号必须大于0")]
    public void Should_Throw_When_Adr_Number_Is_Zero_Or_Negative()
    {
        // Arrange & Act & Assert
        var act1 = () => new ArchitectureRuleSet(0);
        var act2 = () => new ArchitectureRuleSet(-1);

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "应该能添加规则")]
    public void Should_Add_Rule()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);

        // Act
        ruleSet.AddRule(3, "最小断言语义规范", RuleSeverity.Governance, RuleScope.Test);

        // Assert
        ruleSet.RuleCount.Should().Be(1);
        ruleSet.Rules.Should().HaveCount(1);
        
        var rule = ruleSet.GetRule(3);
        rule.Should().NotBeNull();
        rule!.Id.ToString().Should().Be("ADR-0907.3");
        rule.Summary.Should().Be("最小断言语义规范");
        rule.Severity.Should().Be(RuleSeverity.Governance);
        rule.Scope.Should().Be(RuleScope.Test);
    }

    [Fact(DisplayName = "应该能添加条款")]
    public void Should_Add_Clause()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);

        // Act
        ruleSet.AddClause(3, 1, "测试类至少包含1个有效断言", "通过静态分析验证");

        // Assert
        ruleSet.ClauseCount.Should().Be(1);
        ruleSet.Clauses.Should().HaveCount(1);
        
        var clause = ruleSet.GetClause(3, 1);
        clause.Should().NotBeNull();
        clause!.Id.ToString().Should().Be("ADR-0907.3.1");
        clause.Condition.Should().Be("测试类至少包含1个有效断言");
        clause.Enforcement.Should().Be("通过静态分析验证");
    }

    [Fact(DisplayName = "不能添加重复的规则")]
    public void Should_Not_Add_Duplicate_Rule()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(3, "规则1", RuleSeverity.Governance, RuleScope.Test);

        // Act & Assert
        var act = () => ruleSet.AddRule(3, "规则2", RuleSeverity.Technical, RuleScope.Module);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ADR-0907.3*已存在*");
    }

    [Fact(DisplayName = "不能添加重复的条款")]
    public void Should_Not_Add_Duplicate_Clause()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddClause(3, 1, "条款1", "执行1");

        // Act & Assert
        var act = () => ruleSet.AddClause(3, 1, "条款2", "执行2");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ADR-0907.3.1*已存在*");
    }

    [Fact(DisplayName = "HasRule 应该正确检查规则是否存在")]
    public void HasRule_Should_Check_Rule_Existence()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(3, "规则", RuleSeverity.Governance, RuleScope.Test);

        // Act & Assert
        ruleSet.HasRule(3).Should().BeTrue();
        ruleSet.HasRule(4).Should().BeFalse();
    }

    [Fact(DisplayName = "HasClause 应该正确检查条款是否存在")]
    public void HasClause_Should_Check_Clause_Existence()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddClause(3, 1, "条款", "执行");

        // Act & Assert
        ruleSet.HasClause(3, 1).Should().BeTrue();
        ruleSet.HasClause(3, 2).Should().BeFalse();
        ruleSet.HasClause(4, 1).Should().BeFalse();
    }

    [Fact(DisplayName = "应该能构建完整的 ADR 规则集")]
    public void Should_Build_Complete_Adr_RuleSet()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);

        // Act - 构建 ADR-907 Rule 3 的规则集
        ruleSet.AddRule(3, "最小断言语义规范", RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(3, 1, "测试类至少包含1个有效断言", "静态分析");
        ruleSet.AddClause(3, 2, "测试方法只能映射一个子规则", "命名检查");
        ruleSet.AddClause(3, 3, "失败信息必须可溯源", "消息格式验证");
        ruleSet.AddClause(3, 4, "禁止形式化断言", "模式匹配检查");

        // Assert
        ruleSet.RuleCount.Should().Be(1);
        ruleSet.ClauseCount.Should().Be(4);
        
        // 验证规则
        var rule = ruleSet.GetRule(3);
        rule.Should().NotBeNull();
        rule!.Id.Level.Should().Be(RuleLevel.Rule);
        
        // 验证所有条款
        for (int i = 1; i <= 4; i++)
        {
            var clause = ruleSet.GetClause(3, i);
            clause.Should().NotBeNull();
            clause!.Id.Level.Should().Be(RuleLevel.Clause);
        }
    }

    [Fact(DisplayName = "不能添加空摘要的规则")]
    public void Should_Not_Add_Rule_With_Empty_Summary()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);

        // Act & Assert
        var act1 = () => ruleSet.AddRule(3, "", RuleSeverity.Governance, RuleScope.Test);
        var act2 = () => ruleSet.AddRule(3, "   ", RuleSeverity.Governance, RuleScope.Test);

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "不能添加空描述的条款")]
    public void Should_Not_Add_Clause_With_Empty_Description()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(907);

        // Act & Assert
        var act1 = () => ruleSet.AddClause(3, 1, "", "执行");
        var act2 = () => ruleSet.AddClause(3, 1, "条件", "");

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }
}
