namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AdrDecisionGenerator 的 Golden 测试
/// 验证生成的内容与标准样本文件的一致性
/// </summary>
public sealed class AdrDecisionGenerator_GoldenTests
{
    private readonly IAdrDecisionGenerator _generator = new AdrDecisionGenerator();

    [Fact]
    public void GenerateDecisionSection_WithAdr907RuleSet_MatchesGoldenSample()
    {
        // Arrange
        var ruleSet = CreateAdr907SampleRuleSet();
        var goldenFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "Specification",
            "Generator",
            "Tests",
            "golden",
            "adr907_sample.md"
        );

        // Act
        var generated = _generator.GenerateDecisionSection(ruleSet);

        // Assert
        generated.Should().NotBeNullOrWhiteSpace();

        // 如果 golden 文件存在，则验证内容匹配
        if (File.Exists(goldenFilePath))
        {
            var expected = File.ReadAllText(goldenFilePath);
            generated.Should().Be(expected, "生成的内容应与 golden 样本文件一致");
        }
        else
        {
            // 如果 golden 文件不存在，输出生成的内容以便创建 golden 文件
            Console.WriteLine("Golden 文件不存在，生成的内容：");
            Console.WriteLine(generated);
        }
    }

    [Fact]
    public void GenerateDecisionSection_StructureMatchesGoldenSample()
    {
        // Arrange
        var ruleSet = CreateAdr907SampleRuleSet();

        // Act
        var generated = _generator.GenerateDecisionSection(ruleSet);

        // Assert - 验证关键结构元素
        generated.Should().Contain("## Decision（裁决）");
        generated.Should().Contain("> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**");
        generated.Should().Contain("### ADR-907_1：ArchitectureTests 的法律地位（Rule）");
        generated.Should().Contain("### ADR-907_2：命名与组织规范（Rule）");
        generated.Should().Contain("#### ADR-907_1_1 唯一自动化执法形式");
        generated.Should().Contain("#### ADR-907_1_2 可执法性要求");
        generated.Should().Contain("#### ADR-907_2_1 独立测试项目要求");
        generated.Should().Contain("#### ADR-907_2_2 ADR 编号目录分组");
    }

    [Fact]
    public void GenerateDecisionSection_WithDifferentOptions_ProducesConsistentStructure()
    {
        // Arrange
        var ruleSet = CreateAdr907SampleRuleSet();
        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = true,
            IncludeWarningNote = true,
            HeaderLevelOffset = 0,
            EscapeMarkdown = true,
            AddBlankLinesBetweenClauses = false
        };

        // Act
        var generated = _generator.GenerateDecisionSection(ruleSet, options);

        // Assert - 确保输出稳定且可重现
        var lines = generated.Split('\n');
        lines[0].Should().Be("## Decision（裁决）");
        lines[2].Should().Be("> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**");
    }

    private static ArchitectureRuleSet CreateAdr907SampleRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(907);

        // Rule 1
        ruleSet.AddRule(1, "ArchitectureTests 的法律地位", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "唯一自动化执法形式", "ArchitectureTests 是 ADR 的唯一自动化执法形式", ClauseExecutionType.StaticAnalysis);
        ruleSet.AddClause(1, 2, "可执法性要求", "任何具备裁决力的 ADR 必须满足以下条件之一", ClauseExecutionType.StaticAnalysis);

        // Rule 2
        ruleSet.AddRule(2, "命名与组织规范", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(2, 1, "独立测试项目要求", "ArchitectureTests 必须集中于独立测试项目", ClauseExecutionType.StaticAnalysis);
        ruleSet.AddClause(2, 2, "ADR 编号目录分组", "测试目录必须按 ADR 编号分组", ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    }
}
