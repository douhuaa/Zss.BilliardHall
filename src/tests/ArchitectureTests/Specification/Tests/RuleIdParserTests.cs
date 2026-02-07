namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// RuleIdParser 的单元测试
/// 验证 RuleId 字符串解析的核心功能
/// </summary>
public sealed class RuleIdParserTests
{
    #region TryParse 测试（宽容模式）

    [Theory(DisplayName = "TryParse 应该正确解析下划线格式的 Rule ID")]
    [InlineData("ADR-001_1", 1, 1, null)]
    [InlineData("ADR-907_3", 907, 3, null)]
    [InlineData("001_1", 1, 1, null)]
    [InlineData("907_3", 907, 3, null)]
    public void TryParse_Should_Parse_Underscore_Rule_Format(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause)
    {
        // Act
        var success = RuleIdParser.TryParse(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.AdrNumber.Should().Be(expectedAdr);
        result.RuleNumber.Should().Be(expectedRule);
        result.ClauseNumber.Should().Be(expectedClause);
        result.IsRule.Should().BeTrue();
        result.IsClause.Should().BeFalse();
    }

    [Theory(DisplayName = "TryParse 应该正确解析下划线格式的 Clause ID")]
    [InlineData("ADR-001_1_1", 1, 1, 1)]
    [InlineData("ADR-907_3_2", 907, 3, 2)]
    [InlineData("001_1_1", 1, 1, 1)]
    [InlineData("907_3_2", 907, 3, 2)]
    public void TryParse_Should_Parse_Underscore_Clause_Format(
        string input,
        int expectedAdr,
        int expectedRule,
        int expectedClause)
    {
        // Act
        var success = RuleIdParser.TryParse(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.AdrNumber.Should().Be(expectedAdr);
        result.RuleNumber.Should().Be(expectedRule);
        result.ClauseNumber.Should().Be(expectedClause);
        result.IsRule.Should().BeFalse();
        result.IsClause.Should().BeTrue();
    }

    [Theory(DisplayName = "TryParse 应该正确解析点号格式的 Rule ID（兼容旧格式）")]
    [InlineData("ADR-001.1", 1, 1, null)]
    [InlineData("ADR-907.3", 907, 3, null)]
    [InlineData("001.1", 1, 1, null)]
    [InlineData("907.3", 907, 3, null)]
    public void TryParse_Should_Parse_Dot_Rule_Format(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause)
    {
        // Act
        var success = RuleIdParser.TryParse(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.AdrNumber.Should().Be(expectedAdr);
        result.RuleNumber.Should().Be(expectedRule);
        result.ClauseNumber.Should().Be(expectedClause);
        result.IsRule.Should().BeTrue();
    }

    [Theory(DisplayName = "TryParse 应该正确解析点号格式的 Clause ID（兼容旧格式）")]
    [InlineData("ADR-001.1.1", 1, 1, 1)]
    [InlineData("ADR-907.3.2", 907, 3, 2)]
    [InlineData("001.1.1", 1, 1, 1)]
    [InlineData("907.3.2", 907, 3, 2)]
    public void TryParse_Should_Parse_Dot_Clause_Format(
        string input,
        int expectedAdr,
        int expectedRule,
        int expectedClause)
    {
        // Act
        var success = RuleIdParser.TryParse(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.AdrNumber.Should().Be(expectedAdr);
        result.RuleNumber.Should().Be(expectedRule);
        result.ClauseNumber.Should().Be(expectedClause);
        result.IsClause.Should().BeTrue();
    }

    [Theory(DisplayName = "TryParse 应该对无效格式返回 false")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("ADR")]
    [InlineData("ADR-abc")]
    [InlineData("abc_123")]
    [InlineData("001")]  // 只有 ADR 编号，缺少 Rule
    [InlineData("ADR-001")]  // 只有 ADR 编号，缺少 Rule
    public void TryParse_Should_Return_False_For_Invalid_Format(string? input)
    {
        // Act
        var success = RuleIdParser.TryParse(input!, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default(ArchitectureRuleId));
    }

    [Theory(DisplayName = "TryParse 应该支持大小写不敏感")]
    [InlineData("adr-001_1")]
    [InlineData("ADR-001_1")]
    [InlineData("Adr-001_1")]
    [InlineData("adr001_1")]
    public void TryParse_Should_Be_Case_Insensitive(string input)
    {
        // Act
        var success = RuleIdParser.TryParse(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.AdrNumber.Should().Be(1);
        result.RuleNumber.Should().Be(1);
    }

    #endregion

    #region ParseStrict 测试（严格模式）

    [Theory(DisplayName = "ParseStrict 应该正确解析有效的 RuleId")]
    [InlineData("ADR-001_1", 1, 1, null)]
    [InlineData("ADR-907_3_2", 907, 3, 2)]
    [InlineData("001.1", 1, 1, null)]
    [InlineData("907.3.2", 907, 3, 2)]
    public void ParseStrict_Should_Parse_Valid_RuleId(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause)
    {
        // Act
        var result = RuleIdParser.ParseStrict(input);

        // Assert
        result.AdrNumber.Should().Be(expectedAdr);
        result.RuleNumber.Should().Be(expectedRule);
        result.ClauseNumber.Should().Be(expectedClause);
    }

    [Theory(DisplayName = "ParseStrict 应该对空字符串抛出 ArgumentException")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseStrict_Should_Throw_For_Empty_Input(string? input)
    {
        // Act
        var act = () => RuleIdParser.ParseStrict(input!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*不能为空*")
            .And.ParamName.Should().Be("ruleId");
    }

    [Theory(DisplayName = "ParseStrict 应该对无效格式抛出 ArgumentException")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("ADR")]
    [InlineData("ADR-abc")]
    [InlineData("abc_123")]
    [InlineData("001")]  // 只有 ADR 编号
    public void ParseStrict_Should_Throw_For_Invalid_Format(string input)
    {
        // Act
        var act = () => RuleIdParser.ParseStrict(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*无效的 RuleId 格式*")
            .And.ParamName.Should().Be("ruleId");
    }

    #endregion

    #region IsValidRuleId 测试

    [Theory(DisplayName = "IsValidRuleId 应该对有效 RuleId 返回 true")]
    [InlineData("ADR-001_1")]
    [InlineData("ADR-907_3_2")]
    [InlineData("001.1")]
    [InlineData("907.3.2")]
    public void IsValidRuleId_Should_Return_True_For_Valid_RuleId(string input)
    {
        // Act
        var result = RuleIdParser.IsValidRuleId(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory(DisplayName = "IsValidRuleId 应该对无效 RuleId 返回 false")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("001")]
    public void IsValidRuleId_Should_Return_False_For_Invalid_RuleId(string? input)
    {
        // Act
        var result = RuleIdParser.IsValidRuleId(input!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region 边界情况测试

    [Theory(DisplayName = "TryParse 和 ParseStrict 对同一有效输入应返回相同结果")]
    [InlineData("ADR-907_3_2")]
    [InlineData("ADR-001_1")]
    [InlineData("907.3.2")]
    public void TryParse_And_ParseStrict_Should_Return_Same_Result(string input)
    {
        // Act
        var tryParseSuccess = RuleIdParser.TryParse(input, out var tryParseResult);
        var strictResult = RuleIdParser.ParseStrict(input);

        // Assert
        tryParseSuccess.Should().BeTrue();
        tryParseResult.Should().Be(strictResult);
    }

    [Theory(DisplayName = "解析结果应该能正确识别 IsRule 和 IsClause")]
    [InlineData("ADR-001_1", true, false)]
    [InlineData("ADR-001_1_1", false, true)]
    [InlineData("907_3", true, false)]
    [InlineData("907_3_2", false, true)]
    public void Parsed_Result_Should_Correctly_Identify_IsRule_And_IsClause(
        string input, bool expectedIsRule, bool expectedIsClause)
    {
        // Arrange & Act
        var result = RuleIdParser.ParseStrict(input);

        // Assert
        result.IsRule.Should().Be(expectedIsRule);
        result.IsClause.Should().Be(expectedIsClause);
    }

    #endregion
}
