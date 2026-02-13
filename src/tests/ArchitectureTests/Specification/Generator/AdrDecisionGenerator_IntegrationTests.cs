namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AdrDecisionGenerator 的集成测试（重构版）
/// - 使用 FluentAssertions 提升断言可读性
/// - 提取辅助方法，减少重复，早期返回简化控制流
/// </summary>
public sealed class AdrDecisionGenerator_IntegrationTests
{
    private readonly IAdrDecisionGenerator _generator = new AdrDecisionGenerator();
    private const string DecisionHeader = "## Decision（裁决）";

    private static string Generate(IAdrDecisionGenerator generator, ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options = null)
        => options is null ? generator.GenerateDecisionSection(ruleSet) : generator.GenerateDecisionSection(ruleSet, options);

    /// <summary>
    /// 更高效的 Markdown 转义（与生成器逻辑保持一致）
    /// </summary>
    private static string EscapeMarkdown(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

        var sb = new StringBuilder(text.Length + 8);
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '\\': sb.Append(@"\\"); break;
                case '`': sb.Append(@"\`"); break;
                case '*': sb.Append(@"\*"); break;
                case '_': sb.Append(@"\_"); break;
                case '[': sb.Append(@"\["); break;
                case ']': sb.Append(@"\]"); break;
                case '<': sb.Append(@"\<"); break;
                case '>': sb.Append(@"\>"); break;
                case '#': sb.Append(@"\#"); break;
                default: sb.Append(ch); break;
            }
        }
        return sb.ToString();
    }

    private static void AssertNotEmptyAndHasDecisionHeader(string result)
    {
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain(DecisionHeader);
    }

    private static void AssertRuleHeadersPresent(ArchitectureRuleSet ruleSet, string result)
    {
        foreach (var rule in ruleSet.Rules)
        {
            var escapedSummary = EscapeMarkdown(rule.Summary);
            var expectedRuleHeader = $"### {rule.Id}：{escapedSummary}（Rule）";
            result.Should().Contain(expectedRuleHeader);
        }
    }

    private static void AssertClausesPresent(ArchitectureRuleSet ruleSet, string result)
    {
        foreach (var clause in ruleSet.Clauses)
        {
            var escapedCondition = EscapeMarkdown(clause.Condition);
            var escapedEnforcement = EscapeMarkdown(clause.Enforcement);

            var expectedClauseHeader = $"#### {clause.Id} {escapedCondition}";
            var expectedEnforcement = $"- {escapedEnforcement}";

            result.Should().Contain(expectedClauseHeader);
            result.Should().Contain(expectedEnforcement);
        }
    }

    [Fact]
    public void GenerateDecisionSection_WithRealAdr907RuleSet_GeneratesValidMarkdown()
    {
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var result = Generate(_generator, ruleSet);

        AssertNotEmptyAndHasDecisionHeader(result);
        result.Should().Contain("⚠️");
        result.Should().Contain("### ADR-907_");
        result.Should().Contain("#### ADR-907_");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(907)]
    [InlineData(910)]
    public void GenerateDecisionSection_WithRealRuleSet_AllSucceed(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        if (ruleSet is null) return; // RuleSet 不存在则跳过

        var result = Generate(_generator, ruleSet);

        AssertNotEmptyAndHasDecisionHeader(result);

        var hasAnyRule = ruleSet.Rules
            .Select(r => r.Id.ToString())
            .Any(id => result.Contains(id, StringComparison.Ordinal));

        hasAnyRule.Should().BeTrue($"生成的内容应该包含 ADR-{adrNumber} 的规则");
    }

    [Fact]
    public void GenerateDecisionSection_WithRealRuleSet_ContainsAllRules()
    {
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var result = Generate(_generator, ruleSet);

        AssertRuleHeadersPresent(ruleSet, result);
    }

    [Fact]
    public void GenerateDecisionSection_WithRealRuleSet_ContainsAllClauses()
    {
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var result = Generate(_generator, ruleSet);

        AssertClausesPresent(ruleSet, result);
    }

    [Fact]
    public void GenerateDecisionSection_WithRealRuleSet_MaintainsCorrectOrder()
    {
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var result = Generate(_generator, ruleSet);

        var orderedRules = ruleSet.Rules.OrderBy(r => r.Id.RuleNumber).ToList();
        var headers = orderedRules.Select(r => $"### {r.Id}：").ToList();

        var indices = headers.Select(h => result.IndexOf(h, StringComparison.Ordinal)).ToArray();
        indices.Should().OnlyContain(i => i >= 0, "生成结果应包含所有规则标题");

        for (int i = 1; i < indices.Length; i++)
            indices[i].Should().BeGreaterThan(indices[i - 1], $"规则 {orderedRules[i].Id} 应在规则 {orderedRules[i - 1].Id} 之后");
    }

    [Fact]
    public void GenerateDecisionSection_CompareWithActualAdrDocument_StructureMatches()
    {
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = true,
            IncludeWarningNote = true
        };

        var result = Generate(_generator, ruleSet, options);
        var lines = result.Split('\n').Select(l => l.Trim()).ToArray();

        lines.Should().Contain(l => l.StartsWith("## Decision"));
        lines.Should().Contain(l => l.StartsWith("### ADR-907_"));
        lines.Should().Contain(l => l.StartsWith("#### ADR-907_"));
        lines.Should().Contain(l => l.StartsWith("- "));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void GenerateDecisionSection_WithVariousConstitutionalRuleSets_GeneratesValidOutput(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        if (ruleSet is null) return;

        var result = Generate(_generator, ruleSet);

        AssertNotEmptyAndHasDecisionHeader(result);
        ruleSet.RuleCount.Should().BeGreaterThan(0, $"ADR-{adrNumber} 应至少有一个规则");
        ruleSet.ClauseCount.Should().BeGreaterThan(0, $"ADR-{adrNumber} 应至少有一个条款");
    }

    [Fact]
    public void GenerateDecisionSection_WithGovernanceRuleSets_AllGenerate()
    {
        var governanceRuleSets = RuleSetRegistry.GetGovernanceRuleSets();

        foreach (var ruleSet in governanceRuleSets)
        {
            var result = Generate(_generator, ruleSet);
            AssertNotEmptyAndHasDecisionHeader(result);
            ruleSet.RuleCount.Should().BeGreaterThan(0, $"治理层 ADR-{ruleSet.AdrNumber} 应至少有一个规则");
        }
    }

    [Fact]
    public void GenerateDecisionSection_OutputCanBeParsedAsMarkdown()
    {
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var result = Generate(_generator, ruleSet);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var document = Markdown.Parse(result, pipeline);

        document.Should().NotBeNull();
        document.Should().HaveCountGreaterThan(0, "生成的 Markdown 应包含至少一个元素");

        var headings = document.Descendants<HeadingBlock>().ToList();
        headings.Should().HaveCountGreaterThan(0, "应该包含至少一个标题");
    }
}
