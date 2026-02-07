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

    [Theory(DisplayName = "ToString 应该按规范格式输出")]
    [InlineData(907, 3, null, "ADR-907_3")]
    [InlineData(907, 3, 2, "ADR-907_3_2")]
    [InlineData(1, 1, null, "ADR-001_1")]
    [InlineData(907, 1, null, "ADR-907_1")]
    [InlineData(900, 1, null, "ADR-900_1")]
    [InlineData(900, 1, 1, "ADR-900_1_1")]
    [InlineData(907, 3, 1, "ADR-907_3_1")]
    public void ToString_Should_Format_Correctly(int adr, int rule, int? clause, string expected)
    {
        // Arrange: 创建 ArchitectureRuleId 对象
        var ruleId = clause.HasValue
            ? ArchitectureRuleId.Clause(adr, rule, clause.Value)
            : ArchitectureRuleId.Rule(adr, rule);

        // Act: 调用 ToString
        var result = ruleId.ToString();

        // Assert: 验证格式
        result.Should().Be(expected);
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
