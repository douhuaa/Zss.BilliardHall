namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;


/// <summary>
/// 验证 ADR-008_1：文档分级与裁决权（Rule）
/// 验证 ADR-008_1_1：文档分级定义
/// 验证 ADR-008_1_2：唯一裁决权原则
/// 验证 ADR-008_1_3：文档分级判定规则
/// </summary>
public sealed class ADR_008_1_Architecture_Tests
{
    private static string RepoRoot => TestEnvironment.RepositoryRoot;
    private static string DocsRoot => Path.Combine(RepoRoot, "docs");

    [Fact(DisplayName = "ADR-008_1_1: ADR 文档必须位于 adr 目录")]
    public void ADR_008_1_1_ADR_Documents_Must_Be_In_Adr_Directory()
    {
        var adrPath = Path.Combine(DocsRoot, "adr");
        var dirMessage = AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
            ruleId: "ADR-008_1_1",
            directoryPath: adrPath,
            directoryDescription: "ADR 目录",
            remediationSteps: new[]
            {
                "创建 docs/adr 目录",
                "将所有 ADR 文档放置在该目录下",
                "参考 ADR-008_1_1 文档分级定义"
            },
            adrReference: "docs/adr/governance/ADR-008-documentation-hierarchy-governance.md");
        Directory.Exists(adrPath).Should().BeTrue(dirMessage);

        var adrFiles = AdrFileFilter.GetAdrFiles(adrPath);
        var fileMessage = AssertionMessageBuilder.Build(
            ruleId: "ADR-008_1_1",
            summary: "未找到任何 ADR 文档",
            currentState: $"在 {adrPath} 目录中未找到符合命名规范的 ADR 文档",
            remediationSteps: new[]
            {
                "ADR 文档应以 'ADR-' 前缀命名",
                "确保文档格式为 Markdown (.md)",
                "参考 ADR-008_1_1 文档分级定义"
            },
            adrReference: "docs/adr/governance/ADR-008-documentation-hierarchy-governance.md");
        adrFiles.Should().NotBeEmpty(fileMessage);
    }

    [Fact(DisplayName = "ADR-008_1_2: 只有 ADR 具备裁决力")]
    public void ADR_008_1_2_Only_ADR_Has_Decision_Authority()
    {
        // 验证 ADR 文档包含裁决性章节
        var adrPath = Path.Combine(DocsRoot, "adr");
        if (!Directory.Exists(adrPath)) return;

        var adrFiles = AdrFileFilter.GetAdrFiles(adrPath).Take(5); // 抽样检查

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            var message = AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-008_1_2",
                filePath: file,
                missingContent: "## Decision",
                remediationSteps: new[]
                {
                    "ADR 文档必须包含 Decision 章节",
                    "Decision 章节定义具有裁决力的规则",
                    "参考 ADR-008_1_2 唯一裁决权原则"
                },
                adrReference: "docs/adr/governance/ADR-008-documentation-hierarchy-governance.md");
            
            // ADR 应该包含 Decision 章节
            content.Should().Contain("## Decision", message);
        }
    }

    [Fact(DisplayName = "ADR-008_1_3: 非 ADR 文档不得定义架构规则")]
    public void ADR_008_1_3_Non_ADR_Documents_Must_Not_Define_Architecture_Rules()
    {
        // 检查 README 和 Guide 文档是否包含裁决性语言
        var readmeFiles = FileSystemTestHelper.GetFilesInDirectory(DocsRoot, "README*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("node_modules"))
            .Take(5);

        var violations = new List<string>();

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(RepoRoot, file);

            // 检查是否包含裁决性语言（但没有声明无裁决力）
            var hasDecisionLanguage =
                content.Contains("必须", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("禁止", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("不允许", StringComparison.OrdinalIgnoreCase);

            var hasDisclaimerDeclaration =
                content.Contains("无裁决力", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("不具备裁决", StringComparison.OrdinalIgnoreCase);

            if (hasDecisionLanguage && !hasDisclaimerDeclaration)
            {
                violations.Add($"  • {fileName} 包含裁决性语言但未声明无裁决力");
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-008_1_3",
            summary: "非 ADR 文档违反分级判定规则",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "非 ADR 文档使用裁决性语言时，必须声明'本文档无裁决力'",
                "或者移除裁决性语言，改为说明性表述",
                "参考 ADR-008_1_3 文档分级判定规则"
            },
            adrReference: "docs/adr/governance/ADR-008-documentation-hierarchy-governance.md");
        violations.Should().BeEmpty(message);
    }
}
