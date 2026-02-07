namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// ADR-907 Rule 1：ArchitectureTests 的法律地位
/// 验证 ArchitectureTests 作为 ADR 的唯一自动化执法形式
/// </summary>
public sealed class ADR_907_1_Architecture_Tests
{
    private const string AdrDocumentPath = "docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md";

    [Fact(DisplayName = "ADR-907_1_1：ArchitectureTests 必须是唯一自动化执法形式")]
    public void ADR_907_1_1_ArchitectureTests_Must_Be_Only_Automated_Enforcement()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(1, 1);

        // Act & Assert
        clause.Should().NotBeNull("ADR-907_1_1 必须在 RuleSet 中定义");
        clause!.Condition.Should().Contain("唯一自动化执法形式",
            Build(
                ruleId: "ADR-907_1_1",
                summary: "ArchitectureTests 必须是 ADR 的唯一自动化执法形式",
                currentState: "规则定义不完整或缺失",
                remediationSteps: new[]
                {
                    "检查 Adr907RuleSet.cs 中是否定义了 Rule 1, Clause 1",
                    "确认 Condition 字段包含 '唯一自动化执法形式' 关键词",
                    "验证该规则的执法方式和执行类型已正确配置"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));

        // 验证执行类型应该是静态分析或约定检查
        clause.ExecutionType.Should().BeOneOf(
            new[] { ClauseExecutionType.StaticAnalysis, ClauseExecutionType.Convention },
            "ADR-907_1_1 应该通过静态分析或约定来执行");
    }

    [Fact(DisplayName = "ADR-907_1_2：可执法性要求 - Final ADR 必须有对应测试或标记为 Non-Enforceable")]
    public void ADR_907_1_2_Final_Adr_Must_Have_Tests_Or_Be_Non_Enforceable()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(1, 2);
        var adrPath = TestEnvironment.AdrPath;

        // Act
        var adrFiles = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README") && !f.Contains("archive") && !f.Contains("proposals"))
            .ToList();

        var finalAdrsWithoutTests = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);

            // 检查是否为 Final 状态
            if (!content.Contains("status: Final", StringComparison.OrdinalIgnoreCase))
                continue;

            // 检查是否标记为 Non-Enforceable
            if (content.Contains("Non-Enforceable", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("non_enforceable", StringComparison.OrdinalIgnoreCase))
                continue;

            // 提取 ADR 编号
            var fileName = Path.GetFileName(adrFile);
            var match = Regex.Match(fileName, @"ADR-(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                continue;

            var adrNumber = int.Parse(match.Groups[1].Value);

            // 检查是否有对应的测试目录或测试类
            var testDirectory = Path.Combine(TestEnvironment.RepositoryRoot, "src", "tests", "ArchitectureTests", $"ADR-{adrNumber}");
            var hasTestDirectory = Directory.Exists(testDirectory);

            if (!hasTestDirectory)
            {
                finalAdrsWithoutTests.Add($"ADR-{adrNumber} ({Path.GetFileName(adrFile)})");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_1_2 必须在 RuleSet 中定义");
        
        finalAdrsWithoutTests.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_1_2",
                summary: "所有 Final 状态的 ADR 必须有对应的 ArchitectureTests 或标记为 Non-Enforceable",
                currentState: $"发现 {finalAdrsWithoutTests.Count} 个 Final ADR 没有对应的测试目录：\n  - " +
                             string.Join("\n  - ", finalAdrsWithoutTests),
                remediationSteps: new[]
                {
                    "为每个 Final ADR 创建对应的测试目录：src/tests/ArchitectureTests/ADR-{编号}/",
                    "或在 ADR 文档中明确标记为 'Non-Enforceable' 如果该 ADR 无法通过自动化测试验证",
                    "参考 ARCHITECTURE-TEST-GUIDELINES.md 了解测试编写规范"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_1_3：禁止文档约束例外 - 不存在拒绝自动化的架构规则")]
    public void ADR_907_1_3_No_Documentation_Only_Constraints()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(1, 3);
        var adrPath = TestEnvironment.AdrPath;

        // Act
        var adrFiles = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README") && !f.Contains("archive") && !f.Contains("proposals"))
            .ToList();

        var documentationOnlyAdrs = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);

            // 检查是否明确声明为"文档专属"或"拒绝自动化"
            if (content.Contains("documentation-only", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("文档专属", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("拒绝自动化", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("no-automation", StringComparison.OrdinalIgnoreCase))
            {
                documentationOnlyAdrs.Add(Path.GetFileName(adrFile));
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_1_3 必须在 RuleSet 中定义");

        documentationOnlyAdrs.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_1_3",
                summary: "不存在声明为'文档专属、拒绝自动化'的架构规则",
                currentState: $"发现 {documentationOnlyAdrs.Count} 个 ADR 声明为文档专属或拒绝自动化：\n  - " +
                             string.Join("\n  - ", documentationOnlyAdrs),
                remediationSteps: new[]
                {
                    "移除 ADR 中关于'文档专属'或'拒绝自动化'的声明",
                    "如果该 ADR 确实无法自动化验证，请使用 'Non-Enforceable' 标记",
                    "或者创建对应的 ArchitectureTests 来验证该 ADR 的约束"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }
}
