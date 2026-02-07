namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// RuleIdParser 的单元测试
/// 验证 RuleId 字符串解析的核心功能
/// </summary>
public sealed class RuleIdParserTests
{
    #region 数据源（物化避免延迟枚举）

    public static IEnumerable<object[]> InvalidInputs { get; } = new List<object[]>
    {
        new object[] { null },
        new object[] { "" },
        new object[] { "   " },
        new object[] { "invalid" },
        new object[] { "ADR-" },
        new object[] { "ADR" },
        new object[] { "ADR-abc" },
        new object[] { "abc_123" },
        new object[] { "001" },
        new object[] { "ADR-001" },
    };

    #endregion

    #region 辅助断言

    private static void AssertParsedResult(
        ArchitectureRuleId result,
        int expectedAdr,
        int expectedRule,
        int? expectedClause,
        bool expectedIsRule,
        bool expectedIsClause)
    {
        result.AdrNumber.Should().Be(expectedAdr);
        result.RuleNumber.Should().Be(expectedRule);
        result.ClauseNumber.Should().Be(expectedClause);
        result.IsRule.Should().Be(expectedIsRule);
        result.IsClause.Should().Be(expectedIsClause);
    }

    private static void AssertTryParseSuccess(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause,
        bool expectedIsRule,
        bool expectedIsClause)
    {
        var success = RuleIdParser.TryParse(input, out var result);
        success.Should().BeTrue($"TryParse 应成功解析 '{input}'");
        AssertParsedResult(result, expectedAdr, expectedRule, expectedClause, expectedIsRule, expectedIsClause);
    }

    #endregion

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
        AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause, expectedIsRule: true, expectedIsClause: false);
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
        AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause, expectedIsRule: false, expectedIsClause: true);
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
        AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause, expectedIsRule: true, expectedIsClause: false);
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
        AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause, expectedIsRule: false, expectedIsClause: true);
    }

    [Theory(DisplayName = "TryParse 应该对无效格式返回 false")]
    [MemberData(nameof(InvalidInputs))]
    public void TryParse_Should_Return_False_For_Invalid_Format(string? input)
    {
        var success = RuleIdParser.TryParse(input!, out var result);
        success.Should().BeFalse($"TryParse 应该失败解析 '{input ?? "null"}'");
        result.Should().Be(default(ArchitectureRuleId));
    }

    [Theory(DisplayName = "TryParse 应该支持大小写不敏感")]
    [InlineData("adr-001_1")]
    [InlineData("ADR-001_1")]
    [InlineData("Adr-001_1")]
    [InlineData("adr001_1")]
    public void TryParse_Should_Be_Case_Insensitive(string input)
    {
        AssertTryParseSuccess(input, expectedAdr: 1, expectedRule: 1, expectedClause: null, expectedIsRule: true, expectedIsClause: false);
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
        var result = RuleIdParser.ParseStrict(input);
        AssertParsedResult(result, expectedAdr, expectedRule, expectedClause, expectedIsRule: expectedClause is null, expectedIsClause: expectedClause is not null);
    }

    [Theory(DisplayName = "ParseStrict 应该对空字符串抛出 ArgumentException")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseStrict_Should_Throw_For_Empty_Input(string? input)
    {
        var act = () => RuleIdParser.ParseStrict(input!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*不能为空*")
            .And.ParamName.Should().Be("ruleId");
    }

    [Theory(DisplayName = "ParseStrict 应该对无效格式抛出 ArgumentException")]
    [MemberData(nameof(InvalidInputs))]
    public void ParseStrict_Should_Throw_For_Invalid_Format(string? input)
    {
        // 过滤掉空输入用不同的测试来断言
        if (string.IsNullOrWhiteSpace(input))
            return;

        var act = () => RuleIdParser.ParseStrict(input!);
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
        RuleIdParser.IsValidRuleId(input).Should().BeTrue();
    }

    [Theory(DisplayName = "IsValidRuleId 应该对无效 RuleId 返回 false")]
    [MemberData(nameof(InvalidInputs))]
    public void IsValidRuleId_Should_Return_False_For_Invalid_RuleId(string? input)
    {
        RuleIdParser.IsValidRuleId(input!).Should().BeFalse();
    }

    #endregion

    #region 边界情况测试

    [Theory(DisplayName = "TryParse 和 ParseStrict 对同一有效输入应返回相同结果")]
    [InlineData("ADR-907_3_2")]
    [InlineData("ADR-001_1")]
    [InlineData("907.3.2")]
    public void TryParse_And_ParseStrict_Should_Return_Same_Result(string input)
    {
        var tryParseSuccess = RuleIdParser.TryParse(input, out var tryParseResult);
        var strictResult = RuleIdParser.ParseStrict(input);

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
        var result = RuleIdParser.ParseStrict(input);
        result.IsRule.Should().Be(expectedIsRule);
        result.IsClause.Should().Be(expectedIsClause);
    }

    #endregion
}
