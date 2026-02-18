namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// RuleIdParser 的单元测试
/// 验证 RuleId 字符串解析的核心功能
/// 
/// 重构说明：
/// - 使用 RuleIdAssertions 辅助类替代重复的断言逻辑
/// - 提取测试数据为静态属性，避免延迟枚举问题
/// - 保持 Theory + InlineData 的数据驱动测试模式
/// </summary>
public sealed class RuleIdParser_Tests
{
    #region 测试数据源（物化避免延迟枚举）

    /// <summary>
    /// 无效输入的测试数据
    /// </summary>
    public static IEnumerable<object[]> InvalidInputs { get; } = new List<object[]>
    {
        new object[] { null! },
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

    #region TryParse 测试（宽容模式）

    [Theory(DisplayName = "TryParse 应该正确解析点号格式的 Rule ID（推荐格式）")]
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
        RuleIdAssertions.AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause);
    }

    [Theory(DisplayName = "TryParse 应该正确解析点号格式的 Clause ID（推荐格式）")]
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
        RuleIdAssertions.AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause);
    }

    [Theory(DisplayName = "TryParse 应该正确解析下划线格式的 Rule ID（兼容旧格式）")]
    [InlineData("ADR-001.1", 1, 1, null)]
    [InlineData("ADR-907.3", 907, 3, null)]
    [InlineData("001_1", 1, 1, null)]
    [InlineData("907_3", 907, 3, null)]
    public void TryParse_Should_Parse_Underscore_Rule_Format(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause)
    {
        RuleIdAssertions.AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause);
    }

    [Theory(DisplayName = "TryParse 应该正确解析下划线格式的 Clause ID（兼容旧格式）")]
    [InlineData("ADR-001.1.1", 1, 1, 1)]
    [InlineData("ADR-907.3.2", 907, 3, 2)]
    [InlineData("001_1_1", 1, 1, 1)]
    [InlineData("907_3_2", 907, 3, 2)]
    public void TryParse_Should_Parse_Underscore_Clause_Format(
        string input,
        int expectedAdr,
        int expectedRule,
        int expectedClause)
    {
        RuleIdAssertions.AssertTryParseSuccess(input, expectedAdr, expectedRule, expectedClause);
    }

    [Theory(DisplayName = "TryParse 应该对无效格式返回 false")]
    [MemberData(nameof(InvalidInputs))]
    public void TryParse_Should_Return_False_For_Invalid_Format(string? input)
    {
        RuleIdAssertions.AssertTryParseFailed(input);
    }

    [Theory(DisplayName = "TryParse 应该支持大小写不敏感")]
    [InlineData("adr-001.1")]
    [InlineData("ADR-001.1")]
    [InlineData("Adr-001.1")]
    [InlineData("adr001.1")]
    public void TryParse_Should_Be_Case_Insensitive(string input)
    {
        RuleIdAssertions.AssertTryParseSuccess(input, expectedAdr: 1, expectedRule: 1, expectedClause: null);
    }

    #endregion

    #region ParseStrict 测试（严格模式）

    [Theory(DisplayName = "ParseStrict 应该正确解析有效的 RuleId")]
    [InlineData("ADR-001.1", 1, 1, null)]
    [InlineData("ADR-907.3.2", 907, 3, 2)]
    [InlineData("001.1", 1, 1, null)]
    [InlineData("907.3.2", 907, 3, 2)]
    [InlineData("ADR-001.1", 1, 1, null)]
    [InlineData("ADR-907.3.2", 907, 3, 2)]
    public void ParseStrict_Should_Parse_Valid_RuleId(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause)
    {
        var result = RuleIdParser.ParseStrict(input);
        RuleIdAssertions.AssertParsedRuleId(result, expectedAdr, expectedRule, expectedClause,
            context: $"ParseStrict('{input}')");
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
    [InlineData("ADR-001.1")]
    [InlineData("ADR-907.3.2")]
    [InlineData("001.1")]
    [InlineData("907.3.2")]
    [InlineData("ADR-001.1")]
    [InlineData("ADR-907.3.2")]
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
    [InlineData("ADR-907.3.2")]
    [InlineData("ADR-001.1")]
    [InlineData("907.3.2")]
    [InlineData("ADR-907.3.2")]
    [InlineData("ADR-001.1")]
    public void TryParse_And_ParseStrict_Should_Return_Same_Result(string input)
    {
        var tryParseSuccess = RuleIdParser.TryParse(input, out var tryParseResult);
        var strictResult = RuleIdParser.ParseStrict(input);

        tryParseSuccess.Should().BeTrue($"TryParse 应成功解析 '{input}'");
        RuleIdAssertions.AssertRuleIdEquals(tryParseResult, strictResult,
            context: $"TryParse 和 ParseStrict 对 '{input}' 的结果");
    }

    [Theory(DisplayName = "解析结果应该能正确识别 IsRule 和 IsClause")]
    [InlineData("ADR-001.1", true, false)]
    [InlineData("ADR-001.1.1", false, true)]
    [InlineData("907.3", true, false)]
    [InlineData("907.3.2", false, true)]
    [InlineData("ADR-001.1", true, false)]
    [InlineData("ADR-001.1.1", false, true)]
    public void Parsed_Result_Should_Correctly_Identify_IsRule_And_IsClause(
        string input, bool expectedIsRule, bool expectedIsClause)
    {
        var result = RuleIdParser.ParseStrict(input);
        result.IsRule.Should().Be(expectedIsRule);
        result.IsClause.Should().Be(expectedIsClause);
    }

    #endregion

    #region 向后兼容性测试

    [Theory(DisplayName = "输入下划线格式应输出点号格式（向后兼容）")]
    [InlineData("ADR-001.1", "ADR-001.1")]
    [InlineData("ADR-907.3.2", "ADR-907.3.2")]
    [InlineData("001_1", "ADR-001.1")]
    [InlineData("907_3_2", "ADR-907.3.2")]
    public void Parsed_Underscore_Format_Should_Output_Dot_Format(string input, string expectedOutput)
    {
        var result = RuleIdParser.ParseStrict(input);
        result.ToString().Should().Be(expectedOutput,
            $"输入 '{input}' 应被解析并输出为 '{expectedOutput}'");
    }

    [Theory(DisplayName = "输入点号格式应输出点号格式")]
    [InlineData("ADR-001.1", "ADR-001.1")]
    [InlineData("ADR-907.3.2", "ADR-907.3.2")]
    [InlineData("001.1", "ADR-001.1")]
    [InlineData("907.3.2", "ADR-907.3.2")]
    public void Parsed_Dot_Format_Should_Output_Dot_Format(string input, string expectedOutput)
    {
        var result = RuleIdParser.ParseStrict(input);
        result.ToString().Should().Be(expectedOutput,
            $"输入 '{input}' 应被解析并输出为 '{expectedOutput}'");
    }

    #endregion
}
