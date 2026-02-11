using System.Security.Cryptography;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AdrDecisionGenerator 的单元测试（重构版）
/// - 使用 FluentAssertions 优化断言可读性
/// - 提取断言辅助方法减少重复
/// </summary>
public sealed class AdrDecisionGenerator_Tests
{
    private readonly IAdrDecisionGenerator _generator = new AdrDecisionGenerator();

    // 常量：便于维护与统一
    private const int AdrNumber = 907;
    private const string SectionHeader = "## Decision（裁决）";
    private const string WarningNote = "⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**";

    #region 辅助方法

    private static ArchitectureRuleSet NewRuleSet(int adrNumber) => new ArchitectureRuleSet(adrNumber);

    private static void AddRuleWithClauses(
        ArchitectureRuleSet ruleSet,
        int ruleNumber,
        string ruleTitle,
        DecisionLevel decisionLevel = DecisionLevel.Must,
        RuleSeverity ruleSeverity = RuleSeverity.Governance,
        RuleScope ruleScope = RuleScope.Test,
        params (int clauseNumber, string clauseTitle, string clauseExecution, ClauseExecutionType execType)[] clauses)
    {
        ruleSet.AddRule(ruleNumber, ruleTitle, decisionLevel, ruleSeverity, ruleScope);
        foreach (var (clauseNumber, clauseTitle, clauseExecution, execType) in clauses)
        {
            ruleSet.AddClause(ruleNumber, clauseNumber, clauseTitle, clauseExecution, execType);
        }
    }

    private static void AddRuleWithClauses(
        ArchitectureRuleSet ruleSet,
        int ruleNumber,
        string ruleTitle,
        params (int clauseNumber, string clauseTitle, string clauseExecution, ClauseExecutionType execType)[] clauses) =>
        AddRuleWithClauses(ruleSet, ruleNumber, ruleTitle, DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test, clauses);

    private static ArchitectureRuleSet CreateRuleSetWithSingleClause(int adr, int ruleNum = 1,
        int clauseNum = 1, string ruleTitle = "规则", string clauseTitle = "条款", string exec = "执行",
        ClauseExecutionType execType = ClauseExecutionType.StaticAnalysis)
    {
        var rs = NewRuleSet(adr);
        AddRuleWithClauses(rs, ruleNum, ruleTitle, (clauseNum, clauseTitle, exec, execType));
        return rs;
    }

    // 断言辅助：集中常用断言
    private static void ShouldContainRuleHeading(string result, int adr, int ruleNum, string ruleTitle)
    {
        var expected = $"### ADR-{adr}_{ruleNum}：{ruleTitle}（Rule）";
        result.Should().Contain(expected);
    }

    private static void ShouldContainClause(string result, int adr, int ruleNum, int clauseNum, string clauseTitle)
    {
        var expected = $"#### ADR-{adr}_{ruleNum}_{clauseNum} {clauseTitle}";
        result.Should().Contain(expected);
    }

    private static void ShouldHaveRuleOrder(string result, int adr, params int[] ruleNumbers)
    {
        var indices = ruleNumbers.Select(n => result.IndexOf($"### ADR-{adr}_{n}", StringComparison.Ordinal)).ToArray();
        for (int i = 1; i < indices.Length; i++)
        {
            indices[i - 1].Should().BeGreaterThanOrEqualTo(0, $"规则 {ruleNumbers[i - 1]} 未找到");
            indices[i].Should().BeGreaterThan(indices[i - 1], $"规则顺序错误: {ruleNumbers[i - 1]} !< {ruleNumbers[i]}");
        }
    }

    private static void ShouldHaveClauseOrder(string result, int adr, int ruleNum, params int[] clauseNumbers)
    {
        var indices = clauseNumbers.Select(n => result.IndexOf($"#### ADR-{adr}_{ruleNum}_{n}", StringComparison.Ordinal)).ToArray();
        for (int i = 1; i < indices.Length; i++)
        {
            indices[i - 1].Should().BeGreaterThanOrEqualTo(0, $"条款 {clauseNumbers[i - 1]} 未找到");
            indices[i].Should().BeGreaterThan(indices[i - 1], $"条款顺序错误: {clauseNumbers[i - 1]} !< {clauseNumbers[i]}");
        }
    }

    #endregion

    #region 基础行为

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
        var ruleSet = NewRuleSet(1);
        AddRuleWithClauses(ruleSet, 1, "测试规则", DecisionLevel.Must, RuleSeverity.Constitutional, RuleScope.Solution,
            (1, "测试条件", "测试执行", ClauseExecutionType.StaticAnalysis));
        DecisionGenerationOptions? options = null;

        Action act = () => _generator.GenerateDecisionSection(ruleSet, options!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateDecisionSection_WithEmptyRuleSet_GeneratesHeaderOnly()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        var result = _generator.GenerateDecisionSection(ruleSet);

        result.Should().Contain(SectionHeader);
        result.Should().Contain(WarningNote);
    }

    #endregion

    #region RuleId / ClauseId 格式（参数化）

    [Theory]
    [InlineData(907, 1, 1, "测试规则", "测试条件")]
    [InlineData(1234, 56, 78, "规则56", "条款78")]
    public void GenerateDecisionSection_UsesCorrectIdFormat(int adr, int ruleNum, int clauseNum, string ruleTitle, string clauseTitle)
    {
        var ruleSet = NewRuleSet(adr);
        AddRuleWithClauses(ruleSet, ruleNum, ruleTitle,
            (clauseNum, clauseTitle, "执行", ClauseExecutionType.Convention));

        var result = _generator.GenerateDecisionSection(ruleSet);

        ShouldContainRuleHeading(result, adr, ruleNum, ruleTitle);
        ShouldContainClause(result, adr, ruleNum, clauseNum, clauseTitle);
    }

    #endregion

    #region 层级与排序

    [Fact]
    public void GenerateDecisionSection_GeneratesCorrectHierarchy()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1",
            (1, "条款1.1", "执行1.1", ClauseExecutionType.StaticAnalysis),
            (2, "条款1.2", "执行1.2", ClauseExecutionType.Convention));

        var result = _generator.GenerateDecisionSection(ruleSet);
        var lines = result.Split('\n').Select(l => l.Trim()).ToList();

        lines.Should().Contain(l => l.StartsWith("## Decision"));
        lines.Should().Contain(l => l.StartsWith($"### ADR-{AdrNumber}_1"));
        lines.Should().Contain(l => l.StartsWith($"#### ADR-{AdrNumber}_1_1"));
        lines.Should().Contain(l => l.StartsWith($"#### ADR-{AdrNumber}_1_2"));
    }

    [Fact]
    public void GenerateDecisionSection_HandlesMultipleRulesAndClausesAndSortsByNumber()
    {
        var ruleSet = NewRuleSet(AdrNumber);

        // 乱序添加，验证排序
        AddRuleWithClauses(ruleSet, 3, "规则3", (1, "条款3.1", "执行3.1", ClauseExecutionType.StaticAnalysis));
        AddRuleWithClauses(ruleSet, 1, "规则1", (1, "条款1.1", "执行1.1", ClauseExecutionType.StaticAnalysis));
        AddRuleWithClauses(ruleSet, 2, "规则2", (1, "条款2.1", "执行2.1", ClauseExecutionType.StaticAnalysis));

        var result = _generator.GenerateDecisionSection(ruleSet);

        ShouldHaveRuleOrder(result, AdrNumber, 1, 2, 3);
    }

    [Fact]
    public void GenerateDecisionSection_SortsClausesByNumber()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1",
            (3, "条款1.3", "执行1.3", ClauseExecutionType.StaticAnalysis),
            (1, "条款1.1", "执行1.1", ClauseExecutionType.StaticAnalysis),
            (2, "条款1.2", "执行1.2", ClauseExecutionType.StaticAnalysis));

        var result = _generator.GenerateDecisionSection(ruleSet);

        ShouldHaveClauseOrder(result, AdrNumber, 1, 1, 2, 3);
    }

    #endregion

    #region 选项行为测试（参数化）

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenerateDecisionSection_IncludeSectionHeader_Respected(bool includeHeader)
    {
        var ruleSet = CreateRuleSetWithSingleClause(AdrNumber);
        var options = new DecisionGenerationOptions { IncludeSectionHeader = includeHeader };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        if (includeHeader)
            result.Should().Contain(SectionHeader);
        else
            result.Should().NotContain(SectionHeader);

        result.Should().Contain($"### ADR-{AdrNumber}_1");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenerateDecisionSection_IncludeWarningNote_Respected(bool includeWarning)
    {
        var ruleSet = CreateRuleSetWithSingleClause(AdrNumber);
        var options = new DecisionGenerationOptions { IncludeWarningNote = includeWarning };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        if (includeWarning)
            result.Should().Contain("⚠️");
        else
            result.Should().NotContain("⚠️");
    }

    [Fact]
    public void GenerateDecisionSection_WithHeaderLevelOffset_AdjustsLevels()
    {
        var ruleSet = CreateRuleSetWithSingleClause(AdrNumber);
        var options = new DecisionGenerationOptions { HeaderLevelOffset = 1 };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        result.Should().Contain("### Decision（裁决）");  // H3 instead of H2
        result.Should().Contain($"#### ADR-{AdrNumber}_1");       // H4 instead of H3
        result.Should().Contain($"##### ADR-{AdrNumber}_1_1");    // H5 instead of H4
    }

    [Fact]
    public void GenerateDecisionSection_WithAddBlankLinesBetweenClauses_AddsBlankLines()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "规则1",
            (1, "条款1.1", "执行1.1", ClauseExecutionType.StaticAnalysis),
            (2, "条款1.2", "执行1.2", ClauseExecutionType.Convention));

        var options = new DecisionGenerationOptions { AddBlankLinesBetweenClauses = true };
        var result = _generator.GenerateDecisionSection(ruleSet, options);

        var lines = result.Split('\n').Select(l => l.Trim()).ToList();
        var idx1 = lines.FindIndex(l => l.StartsWith($"#### ADR-{AdrNumber}_1_1"));
        var idx2 = lines.FindIndex(l => l.StartsWith($"#### ADR-{AdrNumber}_1_2"));

        idx1.Should().BeGreaterThanOrEqualTo(0);
        idx2.Should().BeGreaterThan(idx1);
        (idx2 - idx1).Should().BeGreaterThan(2);
    }

    #endregion

    #region 内容完整性与真实场景

    [Fact]
    public void GenerateDecisionSection_IncludesAllRequiredElements()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则摘要",
            (1, "测试条件描述", "测试执行要求", ClauseExecutionType.StaticAnalysis));

        var result = _generator.GenerateDecisionSection(ruleSet);

        result.Should().Contain(SectionHeader);
        ShouldContainRuleHeading(result, AdrNumber, 1, "测试规则摘要");
        ShouldContainClause(result, AdrNumber, 1, 1, "测试条件描述");
        result.Should().Contain("- 测试执行要求");
    }

    [Fact]
    public void GenerateDecisionSection_PreservesChineseCharacters()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "命名与组织规范",
            (1, "独立测试项目要求", "ArchitectureTests 必须集中于独立测试项目", ClauseExecutionType.Convention));

        var result = _generator.GenerateDecisionSection(ruleSet);

        ShouldContainRuleHeading(result, AdrNumber, 1, "命名与组织规范");
        ShouldContainClause(result, AdrNumber, 1, 1, "独立测试项目要求");
        result.Should().Contain("ArchitectureTests 必须集中于独立测试项目");
    }

    [Fact]
    public void GenerateDecisionSection_WithRealAdr907Structure_GeneratesCorrectFormat()
    {
        var ruleSet = NewRuleSet(AdrNumber);

        AddRuleWithClauses(ruleSet, 1, "ArchitectureTests 的法律地位",
            (1, "唯一自动化执法形式", "ArchitectureTests 是 ADR 的唯一自动化执法形式", ClauseExecutionType.Convention),
            (2, "可执法性要求", "任何具备裁决力的 ADR 必须满足以下条件之一", ClauseExecutionType.Documentation));

        AddRuleWithClauses(ruleSet, 2, "命名与组织规范",
            (1, "独立测试项目要求", "ArchitectureTests 必须集中于独立测试项目", ClauseExecutionType.Convention),
            (2, "ADR 编号目录分组", "测试目录必须按 ADR 编号分组", ClauseExecutionType.Convention));

        var result = _generator.GenerateDecisionSection(ruleSet);

        ShouldContainRuleHeading(result, AdrNumber, 1, "ArchitectureTests 的法律地位");
        ShouldContainClause(result, AdrNumber, 1, 1, "唯一自动化执法形式");
        ShouldContainClause(result, AdrNumber, 1, 2, "可执法性要求");
        ShouldContainRuleHeading(result, AdrNumber, 2, "命名与组织规范");
        ShouldContainClause(result, AdrNumber, 2, 1, "独立测试项目要求");
        ShouldContainClause(result, AdrNumber, 2, 2, "ADR 编号目录分组");
    }

    #endregion

    #region 确定性测试（Deterministic Tests）

    [Fact]
    public void GenerateDecisionSection_IsDeterministic_ForSameInput()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则",
            (1, "条件1", "执行1", ClauseExecutionType.StaticAnalysis),
            (2, "条件2", "执行2", ClauseExecutionType.Convention));

        var output1 = _generator.GenerateDecisionSection(ruleSet);
        var output2 = _generator.GenerateDecisionSection(ruleSet);

        output1.Should().Be(output2);
    }

    [Fact]
    public void GenerateDecisionSection_IsDeterministic_HashComparison()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则",
            (1, "条件1", "执行1", ClauseExecutionType.StaticAnalysis),
            (2, "条件2", "执行2", ClauseExecutionType.Convention));
        AddRuleWithClauses(ruleSet, 2, "测试规则2",
            (1, "条件2.1", "执行2.1", ClauseExecutionType.Runtime));

        var output1 = _generator.GenerateDecisionSection(ruleSet);
        var output2 = _generator.GenerateDecisionSection(ruleSet);

        var hash1 = ComputeSha256Hash(output1);
        var hash2 = ComputeSha256Hash(output2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GenerateDecisionSection_IsDeterministic_WithOptions()
    {
        var ruleSet = NewRuleSet(AdrNumber);
        AddRuleWithClauses(ruleSet, 1, "测试规则",
            (1, "条件1", "执行1", ClauseExecutionType.StaticAnalysis));

        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = true,
            IncludeWarningNote = true,
            HeaderLevelOffset = 1,
            AddBlankLinesBetweenClauses = true
        };

        var output1 = _generator.GenerateDecisionSection(ruleSet, options);
        var output2 = _generator.GenerateDecisionSection(ruleSet, options);

        output1.Should().Be(output2);
    }

    private static string ComputeSha256Hash(string text)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    #endregion
}
