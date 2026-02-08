namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleId 的单元测试
/// 验证强类型规则ID的核心功能
/// </summary>
public sealed class ArchitectureRuleIdTests
{
    private static ArchitectureRuleId CreateRuleId(int adr, int rule) => ArchitectureRuleId.Rule(adr, rule);
    private static ArchitectureRuleId CreateClauseId(int adr, int rule, int clause) => ArchitectureRuleId.Clause(adr, rule, clause);

    [Theory(DisplayName = "Rule 工厂方法应该创建正确的 Rule 级别ID")]
    [InlineData(907, 3)]
    [InlineData(1, 1)]
    [InlineData(900, 2)]
    public void Rule_Factory_Should_Create_Rule_Level_Id(int adr, int rule)
    {
        // Act
        var ruleId = CreateRuleId(adr, rule);

        // Assert - 验证契约而非实现细节
        ruleId.Level.Should().Be(RuleLevel.Rule, "Rule 工厂必须产生 Rule 级别的ID");
        ruleId.ToString().Should().Be($"ADR-{adr:D3}_{rule}", "ToString 是公开契约");
    }

    [Theory(DisplayName = "Clause 工厂方法应该创建正确的 Clause 级别ID")]
    [InlineData(907, 3, 2)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 2, 5)]
    public void Clause_Factory_Should_Create_Clause_Level_Id(int adr, int rule, int clause)
    {
        // Act
        var clauseId = CreateClauseId(adr, rule, clause);

        // Assert - 验证契约而非实现细节
        clauseId.Level.Should().Be(RuleLevel.Clause, "Clause 工厂必须产生 Clause 级别的ID");
        clauseId.ToString().Should().Be($"ADR-{adr:D3}_{rule}_{clause}", "ToString 是公开契约");
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
        // Arrange
        var ruleId = clause.HasValue ? CreateClauseId(adr, rule, clause.Value) : CreateRuleId(adr, rule);

        // Act
        var result = ruleId.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Parse 应该正确解析规则ID字符串")]
    [InlineData("ADR-907_3", RuleLevel.Rule, "ADR-907_3")]
    [InlineData("ADR-907_3_2", RuleLevel.Clause, "ADR-907_3_2")]
    [InlineData("ADR-900_1", RuleLevel.Rule, "ADR-900_1")]
    [InlineData("ADR-900_1_1", RuleLevel.Clause, "ADR-900_1_1")]
    [InlineData("907_3", RuleLevel.Rule, "ADR-907_3")]
    [InlineData("907_3_2", RuleLevel.Clause, "ADR-907_3_2")]
    public void Parse_Should_Parse_RuleId_String_Correctly(string input, RuleLevel expectedLevel, string expectedToString)
    {
        // Act
        var ruleId = ArchitectureRuleId.Parse(input);

        // Assert - 验证契约而非实现细节
        ruleId.Level.Should().Be(expectedLevel, "Level 是类型级别契约");
        ruleId.ToString().Should().Be(expectedToString, "ToString 是序列化契约");
    }

    [Theory(DisplayName = "Parse 应该拒绝非法格式（宪法级防护）")]
    [InlineData("ADR-", "空 RuleId")]
    [InlineData("ADR-907", "缺少 Rule 编号")]
    [InlineData("ADR--3", "双横线")]
    [InlineData("ADR-907__3", "双下划线")]
    [InlineData("", "空字符串")]
    [InlineData("   ", "空白字符")]
    [InlineData("ADR-abc_3", "非数字 ADR")]
    [InlineData("ADR-907_xyz", "非数字 Rule")]
    public void Parse_Should_Reject_Invalid_Format(string input, string reason)
    {
        // Act
        Action act = () => ArchitectureRuleId.Parse(input);

        // Assert - 这是宪法级防护：拒绝污染规则体系
        act.Should().Throw<ArgumentException>(
            $"非法格式必须被拒绝：{reason}")
            .WithMessage("*RuleId*");
    }

    [Theory(DisplayName = "排序契约：ADR → Rule → Clause（宪法级不变量）")]
    [InlineData(
        new[] { "ADR-907_3_2", "ADR-907_1", "ADR-907_3_1", "ADR-900_1", "ADR-900_1_1" },
        new[] { "ADR-900_1", "ADR-900_1_1", "ADR-907_1", "ADR-907_3_1", "ADR-907_3_2" }
    )]
    public void Sorting_Contract_Adr_Then_Rule_Then_Clause(
        string[] input,
        string[] expected)
    {
        // Arrange
        var ids = input.Select(ArchitectureRuleId.Parse).ToArray();

        // Act
        var sorted = ids.OrderBy(x => x).Select(x => x.ToString()).ToArray();

        // Assert - 这是体系基础，不可修改
        sorted.Should().Equal(expected, "排序优先级：ADR → Rule → Clause 是不可破坏的契约");
    }

    [Theory(DisplayName = "排序契约：Rule 必须排在同编号 Clause 之前")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 5, 2)]
    public void Sorting_Contract_Rule_Before_Clause(int adr, int ruleNum, int clauseNum)
    {
        // Arrange
        var rule = CreateRuleId(adr, ruleNum);
        var clause = CreateClauseId(adr, ruleNum, clauseNum);

        // Act
        var comparison = rule.CompareTo(clause);

        // Assert - Rule 优先级高于 Clause 是不可破坏的契约
        comparison.Should().BeLessThan(0, "Rule 必须排在同编号的 Clause 之前（如 ADR-907_3 < ADR-907_3_1）");
    }

    [Theory(DisplayName = "相同的 RuleId 应该被视为相等")]
    [InlineData(907, 3, null)]
    [InlineData(1, 1, null)]
    [InlineData(900, 2, 3)]
    public void Same_RuleIds_Should_Be_Equal(int adr, int rule, int? clause)
    {
        // Arrange
        var ruleId1 = clause.HasValue ? CreateClauseId(adr, rule, clause.Value) : CreateRuleId(adr, rule);
        var ruleId2 = clause.HasValue ? CreateClauseId(adr, rule, clause.Value) : CreateRuleId(adr, rule);

        // Assert
        ruleId1.Should().Be(ruleId2);
        (ruleId1 == ruleId2).Should().BeTrue();
    }

    [Theory(DisplayName = "不同的 RuleId 应该不相等")]
    [InlineData(907, 3, null, 907, 4, null)]
    [InlineData(907, 3, null, 907, 3, 1)]
    [InlineData(1, 1, null, 2, 1, null)]
    [InlineData(907, 3, 1, 907, 3, 2)]
    public void Different_RuleIds_Should_Not_Be_Equal(
        int adr1, int rule1, int? clause1,
        int adr2, int rule2, int? clause2)
    {
        // Arrange
        var ruleId1 = clause1.HasValue ? CreateClauseId(adr1, rule1, clause1.Value) : CreateRuleId(adr1, rule1);
        var ruleId2 = clause2.HasValue ? CreateClauseId(adr2, rule2, clause2.Value) : CreateRuleId(adr2, rule2);

        // Assert
        ruleId1.Should().NotBe(ruleId2);
    }
}

public sealed class ArchitectureRuleIdIdentityInvariants_Tests
{
    [Theory(DisplayName = "不变量：RuleId 表示 ADR 下的规则级别")]
    [InlineData(907, 3)]
    [InlineData(1, 1)]
    [InlineData(900, 2)]
    public void RuleId_Should_Always_Be_Rule_Level(int adr, int rule)
    {
        var id = ArchitectureRuleId.Rule(adr, rule);

        id.Level.Should().Be(RuleLevel.Rule);
        id.ClauseNumber.Should().BeNull();
    }

    [Theory(DisplayName = "不变量：ClauseId 表示 ADR 下的子规则级别")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 2, 5)]
    public void ClauseId_Should_Always_Be_Clause_Level(int adr, int rule, int clause)
    {
        var id = ArchitectureRuleId.Clause(adr, rule, clause);

        id.Level.Should().Be(RuleLevel.Clause);
        id.ClauseNumber.Should().Be(clause);
    }
}

public sealed class ArchitectureRuleIdRepresentationInvariants_Tests
{
    [Theory(DisplayName = "不变量：RuleId 的字符串表示是稳定规范格式")]
    [InlineData(907, 3, "ADR-907_3")]
    [InlineData(1, 1, "ADR-001_1")]
    [InlineData(900, 2, "ADR-900_2")]
    public void RuleId_ToString_Should_Follow_Spec(int adr, int rule, string expected)
    {
        var id = ArchitectureRuleId.Rule(adr, rule);

        id.ToString().Should().Be(expected);
    }

    [Theory(DisplayName = "不变量：ClauseId 的字符串表示是稳定规范格式")]
    [InlineData(907, 3, 1, "ADR-907_3_1")]
    [InlineData(900, 1, 1, "ADR-900_1_1")]
    public void ClauseId_ToString_Should_Follow_Spec(int adr, int rule, int clause, string expected)
    {
        var id = ArchitectureRuleId.Clause(adr, rule, clause);

        id.ToString().Should().Be(expected);
    }
}

public sealed class ArchitectureRuleIdOrderingInvariants_Tests
{
    [Theory(DisplayName = "不变量：排序顺序为 ADR → Rule → Clause")]
    [InlineData(
    new[] { "ADR-907_3_2", "ADR-907_1", "ADR-907_3_1", "ADR-900_1", "ADR-900_1_1" },
    new[] { "ADR-900_1", "ADR-900_1_1", "ADR-907_1", "ADR-907_3_1", "ADR-907_3_2" }
    )]
    public void RuleIds_Should_Sort_By_Adr_Then_Rule_Then_Clause(
        string[] input,
        string[] expected)
    {
        var ids = input.Select(ArchitectureRuleId.Parse);

        ids.OrderBy(x => x)
            .Select(x => x.ToString())
            .Should()
            .Equal(expected);
    }

    [Theory(DisplayName = "不变量：同编号下 Rule 永远排在 Clause 之前")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    public void Rule_Should_Always_Come_Before_Its_Clause(int adr, int rule, int clause)
    {
        var ruleId = ArchitectureRuleId.Rule(adr, rule);
        var clauseId = ArchitectureRuleId.Clause(adr, rule, clause);

        ruleId.CompareTo(clauseId).Should().BeLessThan(0);
    }
}

public sealed class ArchitectureRuleIdParsingInvariants_Tests
{
    [Theory(DisplayName = "不变量：合法字符串必须可被解析")]
    [InlineData("ADR-907_3")]
    [InlineData("ADR-907_3_1")]
    [InlineData("907_3")]
    [InlineData("907_3_1")]
    public void Parse_Should_Accept_Valid_Formats(string input)
    {
        var id = ArchitectureRuleId.Parse(input);

        id.Should().NotBeNull();
    }

    [Theory(DisplayName = "不变量：非法格式必须被拒绝")]
    [InlineData("ADR-")]
    [InlineData("ADR-907")]
    [InlineData("ADR-907__3")]
    [InlineData("ADR-907_3_")]
    [InlineData("ADR--3")]
    public void Parse_Should_Reject_Invalid_Formats(string input)
    {
        Action act = () => ArchitectureRuleId.Parse(input);

        act.Should().Throw<ArgumentException>();
    }
}


