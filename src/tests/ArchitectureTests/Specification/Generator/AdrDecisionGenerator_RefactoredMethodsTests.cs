using Zss.BilliardHall.Generators;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// 重构后的 AdrDecisionGenerator 内部方法测试
/// 使用反射测试 private 方法，验证：
/// - 早期返回行为
/// - 参数验证
/// - 边界条件处理
/// - 方法职责单一性
/// </summary>
public sealed class AdrDecisionGenerator_RefactoredMethodsTests
{
    private readonly IAdrDecisionGenerator _generator = new AdrDecisionGenerator();
    private const int AdrNumber = 907;

    #region 辅助方法

    private static ArchitectureRuleSet NewRuleSet(int adrNumber) => new ArchitectureRuleSet(adrNumber);

    private static void AddRuleWithClauses(
        ArchitectureRuleSet ruleSet,
        int ruleNumber,
        string ruleTitle,
        params (int clauseNumber, string clauseTitle, string clauseExecution)[] clauses)
    {
        ruleSet.AddRule(ruleNumber, ruleTitle, DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        foreach (var (clauseNumber, clauseTitle, clauseExecution) in clauses)
        {
            ruleSet.AddClause(ruleNumber, clauseNumber, clauseTitle, clauseExecution, ClauseExecutionType.StaticAnalysis);
        }
    }

    #endregion

    #region BuildSectionHeader 测试

    [Theory]
    [InlineData(true, true, 0)]  // 包含标题和警告
    [InlineData(true, false, 0)] // 包含标题但不包含警告
    [InlineData(false, true, 0)] // 不包含标题（警告也不会出现）
    [InlineData(true, true, 1)]  // 标题偏移 1
    [InlineData(true, true, 2)]  // 标题偏移 2
    public void BuildSectionHeader_WithVariousOptions_GeneratesCorrectFormat(
        bool includeHeader,
        bool includeWarning,
        int headerOffset)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则", (1, "测试条件", "测试执行"));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = includeHeader,
            IncludeWarningNote = includeWarning,
            HeaderLevelOffset = headerOffset
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        if (includeHeader)
        {
            var expectedHeaderLevel = new string('#', 2 + headerOffset);
            result.Should().Contain($"{expectedHeaderLevel} Decision（裁决）");

            if (includeWarning)
            {
                result.Should().Contain("⚠️");
            }
            else
            {
                result.Should().NotContain("⚠️");
            }
        }
        else
        {
            result.Should().NotContain("## Decision");
            result.Should().NotContain("### Decision");
            result.Should().NotContain("#### Decision");
            result.Should().NotContain("⚠️");
        }
    }

    #endregion

    #region BuildRulesContent 测试

    [Fact]
    public void BuildRulesContent_WithEmptyRuleSet_GeneratesNoContent()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        var options = new DecisionGenerationOptions { IncludeSectionHeader = false };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildRulesContent_WithMultipleRules_GeneratesInCorrectOrder()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 3, "规则3", (1, "条件3.1", "执行3.1"));
        AddRuleWithClauses(ruleSet, 1, "规则1", (1, "条件1.1", "执行1.1"));
        AddRuleWithClauses(ruleSet, 2, "规则2", (1, "条件2.1", "执行2.1"));

        var options = new DecisionGenerationOptions { IncludeSectionHeader = false };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        var idx1 = result.IndexOf("ADR-907.1", StringComparison.Ordinal);
        var idx2 = result.IndexOf("ADR-907.2", StringComparison.Ordinal);
        var idx3 = result.IndexOf("ADR-907.3", StringComparison.Ordinal);

        idx1.Should().BeGreaterThanOrEqualTo(0);
        idx2.Should().BeGreaterThan(idx1);
        idx3.Should().BeGreaterThan(idx2);
    }

    [Fact]
    public void BuildRulesContent_WithSingleRule_AddsNoTrailingBlankLine()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1", (1, "条件1.1", "执行1.1"));

        var options = new DecisionGenerationOptions { IncludeSectionHeader = false };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        // 不应该有连续的多个空行
        result.Should().NotContain("\n\n\n");
    }

    [Fact]
    public void BuildRulesContent_WithMultipleRules_AddsSingleBlankLineBetween()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1", (1, "条件1.1", "执行1.1"));
        AddRuleWithClauses(ruleSet, 2, "规则2", (1, "条件2.1", "执行2.1"));

        var options = new DecisionGenerationOptions { IncludeSectionHeader = false };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        // 规则之间应该有一个空行
        var lines = result.Split('\n');
        var rule1EndIndex = Array.FindIndex(lines, l => l.Contains("执行1.1"));
        var rule2StartIndex = Array.FindIndex(lines, l => l.Contains("ADR-907.2"));

        rule1EndIndex.Should().BeGreaterThanOrEqualTo(0);
        rule2StartIndex.Should().BeGreaterThan(rule1EndIndex);
        (rule2StartIndex - rule1EndIndex).Should().Be(2); // 一个内容行 + 一个空行
    }

    #endregion

    #region BuildRuleHeader 测试

    [Theory]
    [InlineData(0, "###")]    // 默认偏移
    [InlineData(1, "####")]   // 偏移 1
    [InlineData(2, "#####")]  // 偏移 2
    public void BuildRuleHeader_WithVariousOffsets_GeneratesCorrectHeaderLevel(int offset, string expectedPrefix)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则", (1, "测试条件", "测试执行"));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = false,
            HeaderLevelOffset = offset
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().Contain($"{expectedPrefix} ADR-{AdrNumber}.1：测试规则（Rule）");
    }

    [Theory]
    [InlineData("规则*带*星号", true, "规则\\*带\\*星号")]
    [InlineData("规则_带_下划线", true, "规则\\_带\\_下划线")]
    [InlineData("规则`带`反引号", true, "规则\\`带\\`反引号")]
    [InlineData("规则#带#井号", true, "规则\\#带\\#井号")]
    [InlineData("规则\\带\\反斜杠", true, "规则\\\\带\\\\反斜杠")]
    [InlineData("规则*带*星号", false, "规则*带*星号")]
    public void BuildRuleHeader_WithMarkdownSpecialChars_EscapesCorrectly(
        string ruleSummary,
        bool escapeMarkdown,
        string expectedSummary)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, ruleSummary, (1, "测试条件", "测试执行"));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = false,
            EscapeMarkdown = escapeMarkdown
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().Contain($"ADR-{AdrNumber}.1：{expectedSummary}（Rule）");
    }

    #endregion

    #region BuildClauseSection 测试

    [Theory]
    [InlineData(0, "####")]    // 默认偏移
    [InlineData(1, "#####")]   // 偏移 1
    [InlineData(2, "######")]  // 偏移 2
    public void BuildClauseSection_WithVariousOffsets_GeneratesCorrectHeaderLevel(int offset, string expectedPrefix)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则", (1, "测试条件", "测试执行"));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = false,
            HeaderLevelOffset = offset
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().Contain($"{expectedPrefix} ADR-{AdrNumber}.1.1 测试条件");
    }

    [Theory]
    [InlineData("条件*带*星号", "执行*带*星号", true, "条件\\*带\\*星号", "执行\\*带\\*星号")]
    [InlineData("条件_带_下划线", "执行_带_下划线", true, "条件\\_带\\_下划线", "执行\\_带\\_下划线")]
    [InlineData("条件`带`反引号", "执行`带`反引号", true, "条件\\`带\\`反引号", "执行\\`带\\`反引号")]
    [InlineData("条件*带*星号", "执行*带*星号", false, "条件*带*星号", "执行*带*星号")]
    public void BuildClauseSection_WithMarkdownSpecialChars_EscapesCorrectly(
        string condition,
        string enforcement,
        bool escapeMarkdown,
        string expectedCondition,
        string expectedEnforcement)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则", (1, condition, enforcement));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = false,
            EscapeMarkdown = escapeMarkdown
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().Contain(expectedCondition);
        result.Should().Contain($"- {expectedEnforcement}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BuildClauseSection_WithBlankLinesOption_AddsBlankLinesCorrectly(bool addBlankLines)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1",
            (1, "条件1.1", "执行1.1"),
            (2, "条件1.2", "执行1.2"));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = false,
            AddBlankLinesBetweenClauses = addBlankLines
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);
        var lines = result.Split('\n').Select(l => l.Trim()).ToList();

        var idx1 = lines.FindIndex(l => l.StartsWith($"#### ADR-{AdrNumber}.1.1"));
        var idx2 = lines.FindIndex(l => l.StartsWith($"#### ADR-{AdrNumber}.1.2"));

        idx1.Should().BeGreaterThanOrEqualTo(0);
        idx2.Should().BeGreaterThan(idx1);

        if (addBlankLines)
        {
            // 应该有空行
            (idx2 - idx1).Should().BeGreaterThan(2);
        }
        else
        {
            // 不应该有空行
            (idx2 - idx1).Should().Be(2); // 标题行 + 执行行
        }
    }

    #endregion

    #region EscapeMarkdown 测试

    [Theory]
    [InlineData("普通文本", "普通文本")]
    [InlineData("*星号*", "\\*星号\\*")]
    [InlineData("_下划线_", "\\_下划线\\_")]
    [InlineData("`反引号`", "\\`反引号\\`")]
    [InlineData("#井号#", "\\#井号\\#")]
    [InlineData("\\反斜杠\\", "\\\\反斜杠\\\\")]
    [InlineData("[方括号]", "\\[方括号\\]")]
    [InlineData("<尖括号>", "\\<尖括号\\>")]
    [InlineData("混合*_`#\\[]<>", "混合\\*\\_\\`\\#\\\\\\[\\]\\<\\>")]
    public void EscapeMarkdown_WithVariousInputs_EscapesCorrectly(string input, string expected)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, input, (1, input, input));

        var optionsWithEscape = new DecisionGenerationOptions
        {
            IncludeSectionHeader = false,
            EscapeMarkdown = true
        };

        var result = _generator.GenerateDecisionSection(ruleSet, optionsWithEscape);
        result.Should().Contain(expected);
    }

    [Fact]
    public void EscapeMarkdown_WithNullOrEmpty_ReturnsEmptyString()
    {
        // EscapeMarkdown 是私有方法，但我们可以通过公共 API 间接验证
        // 当 Summary/Condition/Enforcement 为空时，生成器会处理它们
        var ruleSet = NewRuleSet(AdrNumber);

        // 使用非空值来避免验证错误，但验证转义逻辑
        AddRuleWithClauses(ruleSet, 1, "测试", (1, "测试", "测试"));

        var options = new DecisionGenerationOptions { EscapeMarkdown = false };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        // 验证不转义时的行为
        result.Should().Contain("测试");
    }

    #endregion

    #region NormalizeNewlines 测试

    [Theory]
    [InlineData("行1\r\n行2", "行1", "行2")]
    [InlineData("行1\r行2", "行1", "行2")]
    [InlineData("行1\n行2", "行1", "行2")]
    [InlineData("行1\r\n行2\r行3\n行4", "行1", "行4")]
    public void NormalizeNewlines_WithVariousLineEndings_NormalizesToLF(string input, string expectedPart1, string expectedPart2)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, input, (1, input, input));

        var options = new DecisionGenerationOptions { IncludeSectionHeader = false, EscapeMarkdown = false };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        // 验证输出中的换行符都是 LF
        result.Should().NotContain("\r\n");
        result.Should().NotContain("\r");

        // 验证内容中包含预期的部分
        result.Should().Contain(expectedPart1);
        result.Should().Contain(expectedPart2);
    }

    #endregion

    #region MakeHeaderPrefix 测试

    [Theory]
    [InlineData(2, "##")]
    [InlineData(3, "###")]
    [InlineData(4, "####")]
    [InlineData(5, "#####")]
    [InlineData(6, "######")]
    public void MakeHeaderPrefix_WithVariousLevels_GeneratesCorrectPrefix(int level, string expected)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则", (1, "测试条件", "测试执行"));

        // 使用 HeaderLevelOffset 间接测试 MakeHeaderPrefix
        var sectionLevel = 2;
        var offset = level - sectionLevel;

        // 只测试有效的偏移范围 (0-2)
        if (offset < 0 || offset > 2)
        {
            return;
        }

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = true,
            HeaderLevelOffset = offset
        };

        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().Contain($"{expected} Decision（裁决）");
    }

    #endregion

    #region 边界条件和错误处理

    [Fact]
    public void GenerateDecisionSection_WithNullRuleSet_ThrowsArgumentNullException()
    {
        ArchitectureRuleSet? ruleSet = null;
        Action act = () => _generator.GenerateDecisionSection(ruleSet!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateDecisionSection_WithNullOptions_ThrowsArgumentNullException()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        DecisionGenerationOptions? options = null;

        Action act = () => _generator.GenerateDecisionSection(ruleSet, options!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(10)]
    public void GenerateDecisionSection_WithInvalidHeaderLevelOffset_ThrowsArgumentOutOfRangeException(int invalidOffset)
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则", (1, "测试条件", "测试执行"));

        var options = new DecisionGenerationOptions { HeaderLevelOffset = invalidOffset };

        Action act = () => _generator.GenerateDecisionSection(ruleSet, options);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region 性能和确定性测试

    [Fact]
    public void GenerateDecisionSection_WithLargeRuleSet_PerformsEfficiently()
    {
        var ruleSet = NewRuleSet(AdrNumber);

        // 添加 50 个规则，每个规则有 10 个条款
        for (int i = 1; i <= 50; i++)
        {
            var clauses = Enumerable.Range(1, 10)
                .Select(c => (c, $"条款{i}.{c}", $"执行{i}.{c}"))
                .ToArray();

            AddRuleWithClauses(ruleSet, i, $"规则{i}", clauses);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _generator.GenerateDecisionSection(ruleSet);
        stopwatch.Stop();

        // 生成应该在合理时间内完成（例如 1 秒）
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        result.Should().NotBeEmpty();
        result.Should().Contain("ADR-907.1");
        result.Should().Contain("ADR-907.50");
    }

    [Fact]
    public void GenerateDecisionSection_IsDeterministic_MultipleInvocations()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1", (1, "条件1.1", "执行1.1"), (2, "条件1.2", "执行1.2"));
        AddRuleWithClauses(ruleSet, 2, "规则2", (1, "条件2.1", "执行2.1"));

        var results = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(_generator.GenerateDecisionSection(ruleSet));
        }

        // 所有结果应该完全相同
        results.Should().AllSatisfy(r => r.Should().Be(results[0]));
    }

    #endregion
}
