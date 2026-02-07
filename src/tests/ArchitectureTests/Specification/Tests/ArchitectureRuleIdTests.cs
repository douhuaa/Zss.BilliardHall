namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleId 的单元测试
/// 验证强类型规则ID的核心功能
/// </summary>
public sealed class ArchitectureRuleIdTests
{
    [Theory(DisplayName = "Rule 工厂方法应该创建正确的 Rule 级别ID")]
    [InlineData(907, 3)]
    [InlineData(1, 1)]
    [InlineData(900, 2)]
    public void Rule_Factory_Should_Create_Rule_Level_Id(int adr, int rule)
    {
        // Arrange & Act
        var ruleId = ArchitectureRuleId.Rule(adr, rule);

        // Assert
        ruleId.AdrNumber.Should().Be(adr);
        ruleId.RuleNumber.Should().Be(rule);
        ruleId.ClauseNumber.Should().BeNull();
        ruleId.Level.Should().Be(RuleLevel.Rule);
    }

    [Theory(DisplayName = "Clause 工厂方法应该创建正确的 Clause 级别ID")]
    [InlineData(907, 3, 2)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 2, 5)]
    public void Clause_Factory_Should_Create_Clause_Level_Id(int adr, int rule, int clause)
    {
        // Arrange & Act
        var clauseId = ArchitectureRuleId.Clause(adr, rule, clause);

        // Assert
        clauseId.AdrNumber.Should().Be(adr);
        clauseId.RuleNumber.Should().Be(rule);
        clauseId.ClauseNumber.Should().Be(clause);
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

    [Theory(DisplayName = "相同的 RuleId 应该被视为相等")]
    [InlineData(907, 3, null)]
    [InlineData(1, 1, null)]
    [InlineData(900, 2, 3)]
    public void Same_RuleIds_Should_Be_Equal(int adr, int rule, int? clause)
    {
        // Arrange
        var ruleId1 = clause.HasValue 
            ? ArchitectureRuleId.Clause(adr, rule, clause.Value)
            : ArchitectureRuleId.Rule(adr, rule);
        var ruleId2 = clause.HasValue 
            ? ArchitectureRuleId.Clause(adr, rule, clause.Value)
            : ArchitectureRuleId.Rule(adr, rule);

        // Act & Assert
        ruleId1.Should().Be(ruleId2);
        (ruleId1 == ruleId2).Should().BeTrue();
    }

    [Theory(DisplayName = "不同的 RuleId 应该不相等")]
    [InlineData(907, 3, null, 907, 4, null)]  // 不同的 Rule number
    [InlineData(907, 3, null, 907, 3, 1)]     // Rule vs Clause
    [InlineData(1, 1, null, 2, 1, null)]      // 不同的 ADR
    [InlineData(907, 3, 1, 907, 3, 2)]        // 不同的 Clause number
    public void Different_RuleIds_Should_Not_Be_Equal(
        int adr1, int rule1, int? clause1,
        int adr2, int rule2, int? clause2)
    {
        // Arrange
        var ruleId1 = clause1.HasValue
            ? ArchitectureRuleId.Clause(adr1, rule1, clause1.Value)
            : ArchitectureRuleId.Rule(adr1, rule1);
        var ruleId2 = clause2.HasValue
            ? ArchitectureRuleId.Clause(adr2, rule2, clause2.Value)
            : ArchitectureRuleId.Rule(adr2, rule2);

        // Act & Assert
        ruleId1.Should().NotBe(ruleId2);
    }

    [Theory(DisplayName = "Rule 和同编号的 Clause 排序时 Rule 应该在前")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 5, 2)]
    public void Rule_Should_Come_Before_Clause_In_Sorting(int adr, int ruleNum, int clauseNum)
    {
        // Arrange
        var rule = ArchitectureRuleId.Rule(adr, ruleNum);
        var clause = ArchitectureRuleId.Clause(adr, ruleNum, clauseNum);

        // Act
        var comparison = rule.CompareTo(clause);

        // Assert
        comparison.Should().BeLessThan(0, "Rule 应该排在 Clause 之前");
    }
}
