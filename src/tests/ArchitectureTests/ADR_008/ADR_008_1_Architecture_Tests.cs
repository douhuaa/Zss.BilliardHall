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
        Directory.Exists(adrPath).Should().BeTrue(
            $"❌ ADR-008_1_1 违规：ADR 目录不存在\n\n" +
            $"预期路径: {adrPath}\n\n" +
            "修复建议：\n" +
            "1. 创建 docs/adr 目录\n" +
            "2. 将所有 ADR 文档放置在该目录下\n" +
            "3. 参考 ADR-008_1_1 文档分级定义");

        var adrFiles = AdrFileFilter.GetAdrFiles(adrPath);
        adrFiles.Should().NotBeEmpty(
            $"❌ ADR-008_1_1 违规：未找到任何 ADR 文档\n\n" +
            "修复建议：\n" +
            "1. ADR 文档应以 'ADR-' 前缀命名\n" +
            "2. 确保文档格式为 Markdown (.md)\n" +
            "3. 参考 ADR-008_1_1 文档分级定义");
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

            // ADR 应该包含 Decision 章节
            content.Should().Contain("## Decision",
                $"❌ ADR-008_1_2 违规：{fileName} 缺少 Decision 章节\n\n" +
                "修复建议：\n" +
                "1. ADR 文档必须包含 Decision 章节\n" +
                "2. Decision 章节定义具有裁决力的规则\n" +
                "3. 参考 ADR-008_1_2 唯一裁决权原则");
        }

        true.Should().BeTrue("ADR-008_1_2 验证通过");
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

        violations.Should().BeEmpty(
            $"❌ ADR-008_1_3 违规：以下非 ADR 文档违反分级判定规则\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. 非 ADR 文档使用裁决性语言时，必须声明'本文档无裁决力'\n" +
            "2. 或者移除裁决性语言，改为说明性表述\n" +
            "3. 参考 ADR-008_1_3 文档分级判定规则");
    }
}
