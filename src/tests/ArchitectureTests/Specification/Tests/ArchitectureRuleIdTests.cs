namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleId 的单元测试
/// 验证强类型规则ID的核心功能
/// </summary>
public sealed class ArchitectureRuleIdTests
{
    [Fact(DisplayName = "Rule 工厂方法应该创建正确的 Rule 级别ID")]
    public void Rule_Factory_Should_Create_Rule_Level_Id()
    {
        // Arrange & Act
        var ruleId = ArchitectureRuleId.Rule(907, 3);

        // Assert
        ruleId.AdrNumber.Should().Be(907);
        ruleId.RuleNumber.Should().Be(3);
        ruleId.ClauseNumber.Should().BeNull();
        ruleId.Level.Should().Be(RuleLevel.Rule);
    }

    [Fact(DisplayName = "Clause 工厂方法应该创建正确的 Clause 级别ID")]
    public void Clause_Factory_Should_Create_Clause_Level_Id()
    {
        // Arrange & Act
        var clauseId = ArchitectureRuleId.Clause(907, 3, 2);

        // Assert
        clauseId.AdrNumber.Should().Be(907);
        clauseId.RuleNumber.Should().Be(3);
        clauseId.ClauseNumber.Should().Be(2);
        clauseId.Level.Should().Be(RuleLevel.Clause);
    }

    [Fact(DisplayName = "Rule ToString 应该输出规范格式 ADR-907_3")]
    public void Rule_ToString_Should_Output_Standard_Format()
    {
        // Arrange
        var ruleId = ArchitectureRuleId.Rule(907, 3);

        // Act
        var result = ruleId.ToString();

        // Assert
        result.Should().Be("ADR-907_3");
    }

    [Fact(DisplayName = "Clause ToString 应该输出规范格式 ADR-907_3_2")]
    public void Clause_ToString_Should_Output_Standard_Format()
    {
        // Arrange
        var clauseId = ArchitectureRuleId.Clause(907, 3, 2);

        // Act
        var result = clauseId.ToString();

        // Assert
        result.Should().Be("ADR-907_3_2");
    }

    [Fact(DisplayName = "ADR 编号应该正确补零到3位")]
    public void Adr_Number_Should_Be_Padded_To_Three_Digits()
    {
        // Arrange
        var rule1 = ArchitectureRuleId.Rule(1, 1);
        var rule907 = ArchitectureRuleId.Rule(907, 1);

        // Act & Assert
        rule1.ToString().Should().Be("ADR-001_1");
        rule907.ToString().Should().Be("ADR-907_1");
    }

    [Fact(DisplayName = "CompareTo 应该按 ADR、Rule、Clause 顺序排序")]
    public void CompareTo_Should_Sort_By_Adr_Rule_Clause()
    {
        // Arrange
        var ids = new[]
        {
            ArchitectureRuleId.Clause(907, 3, 2),
            ArchitectureRuleId.Rule(907, 1),
            ArchitectureRuleId.Clause(907, 3, 1),
            ArchitectureRuleId.Rule(900, 1),
            ArchitectureRuleId.Clause(900, 1, 1),
        };

        // Act
        var sorted = ids.OrderBy(x => x).ToList();

        // Assert
        sorted[0].ToString().Should().Be("ADR-900_1");
        sorted[1].ToString().Should().Be("ADR-900_1_1");
        sorted[2].ToString().Should().Be("ADR-907_1");
        sorted[3].ToString().Should().Be("ADR-907_3_1");
        sorted[4].ToString().Should().Be("ADR-907_3_2");
    }

    [Fact(DisplayName = "相同的 RuleId 应该被视为相等")]
    public void Same_RuleIds_Should_Be_Equal()
    {
        // Arrange
        var rule1 = ArchitectureRuleId.Rule(907, 3);
        var rule2 = ArchitectureRuleId.Rule(907, 3);

        // Act & Assert
        rule1.Should().Be(rule2);
        (rule1 == rule2).Should().BeTrue();
    }

    [Fact(DisplayName = "不同的 RuleId 应该不相等")]
    public void Different_RuleIds_Should_Not_Be_Equal()
    {
        // Arrange
        var rule1 = ArchitectureRuleId.Rule(907, 3);
        var rule2 = ArchitectureRuleId.Rule(907, 4);
        var clause1 = ArchitectureRuleId.Clause(907, 3, 1);

        // Act & Assert
        rule1.Should().NotBe(rule2);
        rule1.Should().NotBe(clause1);
    }

    [Fact(DisplayName = "Rule 和同编号的 Clause 排序时 Rule 应该在前")]
    public void Rule_Should_Come_Before_Clause_In_Sorting()
    {
        // Arrange
        var rule = ArchitectureRuleId.Rule(907, 3);
        var clause = ArchitectureRuleId.Clause(907, 3, 1);

        // Act
        var comparison = rule.CompareTo(clause);

        // Assert
        comparison.Should().BeLessThan(0, "Rule 应该排在 Clause 之前");
    }
}
