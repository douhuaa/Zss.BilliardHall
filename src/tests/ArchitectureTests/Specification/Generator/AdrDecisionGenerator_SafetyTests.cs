namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// Markdown 转义和选项验证测试
/// </summary>
public sealed class AdrDecisionGenerator_SafetyTests
{
    private readonly IAdrDecisionGenerator _generator = new AdrDecisionGenerator();

    // 辅助：创建一个包含单条规则与单条条款的 ArchitectureRuleSet
    private static ArchitectureRuleSet CreateRuleSet(
        string ruleSummary = "规则",
        string clauseCondition = "条件",
        string enforcement = "执行")
    {
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(1, ruleSummary, DecisionLevel.Must, RuleSeverity.Constitutional, RuleScope.Solution);
        ruleSet.AddClause(1, 1, clauseCondition, enforcement, ClauseExecutionType.Convention);
        return ruleSet;
    }

    // 辅助：调用生成器（可传入选项）
    private string Generate(ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options = null)
        => _generator.GenerateDecisionSection(ruleSet, options ?? new DecisionGenerationOptions());

    #region Markdown 转义测试（参数化）

    [Theory]
    [InlineData("规则包含 *星号* 和 `反引号`", "\\*星号\\*", "\\`反引号\\`")]
    [InlineData("条件包含 [方括号] 和 #井号", "\\[方括号\\]", "\\#井号")]
    [InlineData("执行包含 <标签> 和 _下划线_", "\\<标签\\>", "\\_下划线\\_")]
    public void GenerateDecisionSection_EscapesSpecialCharacters(string sourceText, string expectedEscaped1, string expectedEscaped2)
    {
        // Arrange: 将不同的 sourceText 放到规则摘要 / 条件 / 执行中都能覆盖到对应场景
        // 通过判断文本中是否包含常见关键字符，决定放在哪个字段更合适以覆盖生成逻辑
        ArchitectureRuleSet ruleSet;
        if (sourceText.StartsWith("规则包含", StringComparison.Ordinal))
        {
            ruleSet = CreateRuleSet(ruleSummary: sourceText);
        }
        else if (sourceText.StartsWith("条件包含", StringComparison.Ordinal))
        {
            ruleSet = CreateRuleSet(clauseCondition: sourceText);
        }
        else
        {
            ruleSet = CreateRuleSet(enforcement: sourceText);
        }

        // Act
        var result = Generate(ruleSet);

        // Assert
        result.Should().Contain(expectedEscaped1);
        result.Should().Contain(expectedEscaped2);
    }

    [Fact]
    public void GenerateDecisionSection_CanDisableEscaping()
    {
        // Arrange
        var ruleSet = CreateRuleSet(ruleSummary: "规则包含 *星号*");
        var options = new DecisionGenerationOptions { EscapeMarkdown = false };

        // Act
        var result = Generate(ruleSet, options);

        // Assert - 不应该转义
        result.Should().Contain("*星号*");
        result.Should().NotContain("\\*星号\\*");
    }

    [Fact]
    public void GenerateDecisionSection_EscapesBackslash()
    {
        // Arrange
        var ruleSet = CreateRuleSet(clauseCondition: "路径 C:\\Users\\Test");

        // Act
        var result = Generate(ruleSet);

        // Assert
        // 原始 C:\Users\Test 期望转义为 C:\\Users\\Test（在输出字符串中双写反斜杠）
        result.Should().Contain("C:\\\\Users\\\\Test");
    }

    #endregion

    #region 选项验证测试

    [Fact]
    public void GenerateDecisionSection_ValidatesHeaderLevelOffset_MinValue()
    {
        // Arrange
        var ruleSet = CreateRuleSet();
        var options = new DecisionGenerationOptions { HeaderLevelOffset = -1 };

        // Act & Assert
        Action act = () => Generate(ruleSet, options);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("HeaderLevelOffset");
    }

    [Fact]
    public void GenerateDecisionSection_ValidatesHeaderLevelOffset_MaxValue()
    {
        // Arrange
        var ruleSet = CreateRuleSet();
        var options = new DecisionGenerationOptions { HeaderLevelOffset = 3 };

        // Act & Assert
        Action act = () => Generate(ruleSet, options);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("HeaderLevelOffset");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GenerateDecisionSection_AcceptsValidHeaderLevelOffset(int offset)
    {
        // Arrange
        var ruleSet = CreateRuleSet();
        var options = new DecisionGenerationOptions { HeaderLevelOffset = offset };

        // Act
        Action act = () => Generate(ruleSet, options);

        // Assert - 不应该抛出异常
        act.Should().NotThrow();
    }

    #endregion

    #region 边界条件测试

    [Fact]
    public void GenerateDecisionSection_HandlesVeryLongText()
    {
        // Arrange
        var longText = new string('A', 10000);
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(1, longText, DecisionLevel.Must, RuleSeverity.Constitutional, RuleScope.Solution);
        ruleSet.AddClause(1, 1, longText, longText, ClauseExecutionType.Convention);

        // Act
        var result = Generate(ruleSet);

        // Assert
        result.Should().Contain("A");
        result.Length.Should().BeGreaterThan(10000);
    }

    #endregion
}
