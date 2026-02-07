namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_902;


/// <summary>
/// ADR-902_2: 语义职责边界
/// 验证 ADR 文档的语义职责边界清晰
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-902_2_1: Decision 严格隔离
/// - ADR-902_2_2: ADR 模板不承担语义裁决职责
/// - ADR-902_2_3: Relationships 章节仅承担结构接口职责
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-902-adr-template-structure-contract.md
/// - Prompts: docs/copilot/adr-902.prompts.md
/// </summary>
public sealed class ADR_902_2_Architecture_Tests
{
    private const int MaxAdrFilesToCheckL2 = 20;  // L2 警告级测试

    /// <summary>
    /// ADR-902_2_1: Decision 严格隔离
    /// 验证约束性规则只出现在 Decision 章节（§2.1）
    /// </summary>
    [Fact(DisplayName = "ADR-902_2_1: Decision 严格隔离")]
    public void ADR_902_2_1_Decision_Must_Be_Isolated()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var warnings = new List<string>();

        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(ArchitectureTestSpecification.Adr.Paths.Root);

        Directory.Exists(adrDirectory).Should().BeTrue($"未找到 ADR 文档目录：{adrDirectory}");

        // 裁决性语言模式
        var decisionWords = new[] { "必须", "MUST", "禁止", "FORBIDDEN", "不允许", "NOT ALLOWED", "不得", "SHALL NOT" };

        // 扫描所有 ADR 文档
        var adrFiles = AdrFileFilter
            .GetAdrFiles(adrDirectory)
            .Take(MaxAdrFilesToCheckL2);

        foreach (var file in adrFiles)
        {
            var content = FileSystemTestHelper.ReadFileContent(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 提取非 Decision 章节的内容
            var nonDecisionContent = ExtractNonDecisionContent(content);

            // 移除代码块和引用块
            var contentWithoutCodeBlocks = RemoveCodeBlocks(nonDecisionContent);
            var contentWithoutQuotes = RemoveQuotedSections(contentWithoutCodeBlocks);

            // 检查是否包含裁决性语言
            foreach (var word in decisionWords)
            {
                if (contentWithoutQuotes.Contains(word))
                {
                    warnings.Add($"  ⚠️ {relativePath} - 非 Decision 章节包含裁决词 '{word}'");
                    break;
                }
            }
        }

        // L2 级别：警告但不失败构建
        if (warnings.Any())
        {
            var warningMessage = string.Join("\n", new[]
            {
                "⚠️ ADR-902_2_1 警告（L2）：以下 ADR 可能在非 Decision 章节使用了裁决性语言",
                "",
                "根据 ADR-902_2_1：约束性规则应只出现在 Decision 章节。",
                ""
            }
            .Concat(warnings.Take(10))
            .Concat(new[]
            {
                "",
                "建议：",
                "  1. 将裁决性语句移至 Decision 章节",
                "  2. 在其他章节使用描述性语言",
                "  3. 如果是引用或示例，请用引用块或代码块包围",
                "",
                "注意：这是 L2 警告级别，不会阻断构建。",
                "",
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §2.1"
            }));

            Console.WriteLine(warningMessage);
            Console.WriteLine();
        }

        // L2 警告：测试总是通过，但已输出警告信息
    }

    /// <summary>
    /// ADR-902_2_2: ADR 模板不承担语义裁决职责
    /// 验证 ADR-902 本身不定义语义规则（§2.2）
    /// </summary>
    [Fact(DisplayName = "ADR-902_2_2: ADR 模板不承担语义裁决职责")]
    public void ADR_902_2_2_Template_Must_Not_Define_Semantics()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-902-adr-template-structure-contract.md");

        File.Exists(adrFile).Should().BeTrue($"ADR-902 文档不存在：{adrFile}");

        var content = File.ReadAllText(adrFile);

        // 验证 ADR-902 声明不承担语义职责
        content.Should().Contain("ADR 模板不承担语义裁决职责",
            $"❌ ADR-902_2_2 违规：ADR-902 必须声明不承担语义裁决职责\n\n" +
            $"修复建议：在 Decision 章节中明确声明 ADR-902 仅验证结构，不做语义裁决\n\n" +
            $"参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §2.2");

        // 验证提及由其他 ADR 负责语义（如 ADR-901, ADR-905）
        bool mentionsSemanticADRs = content.Contains("ADR-901") || content.Contains("ADR-905");
        mentionsSemanticADRs.Should().BeTrue(
            $"❌ ADR-902_2_2 违规：ADR-902 应提及负责语义裁决的 ADR（如 ADR-901, ADR-905）\n\n" +
            $"修复建议：在相关章节中引用负责语义裁决的 ADR\n\n" +
            $"参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §2.2");
    }

    /// <summary>
    /// ADR-902_2_3: Relationships 章节仅承担结构接口职责
    /// 验证 ADR-902 对 Relationships 章节仅做结构验证（§2.3）
    /// </summary>
    [Fact(DisplayName = "ADR-902_2_3: Relationships 章节仅承担结构接口职责")]
    public void ADR_902_2_3_Relationships_Section_Only_Handles_Structure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-902-adr-template-structure-contract.md");

        File.Exists(adrFile).Should().BeTrue($"ADR-902 文档不存在：{adrFile}");

        var content = File.ReadAllText(adrFile);

        // 验证 ADR-902 声明 Relationships 章节仅承担结构接口职责
        content.Should().Contain("Relationships 章节仅承担结构接口职责",
            $"❌ ADR-902_2_3 违规：ADR-902 必须声明 Relationships 章节仅承担结构职责\n\n" +
            $"修复建议：在 Decision 章节中明确 Relationships 的职责边界\n\n" +
            $"参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §2.3");

        // 验证提及关系语义由 ADR-940 负责
        content.Should().Contain("ADR-940",
            $"❌ ADR-902_2_3 违规：ADR-902 应提及关系治理由 ADR-940 负责\n\n" +
            $"修复建议：在相关章节中引用 ADR-940（关系治理）\n\n" +
            $"参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §2.3");
    }

    // ========== 辅助方法 ==========


    private static string ExtractNonDecisionContent(string content)
    {
        // 移除 Decision 章节，返回其他内容
        return Regex.Replace(content, @"^##\s+Decision.*?\n.*?(?=^##\s+|\z)",
            string.Empty, RegexOptions.Multiline | RegexOptions.Singleline);
    }

    private static string RemoveCodeBlocks(string content)
    {
        // 移除 Markdown 代码块
        return Regex.Replace(content, @"```[\s\S]*?```", string.Empty);
    }

    private static string RemoveQuotedSections(string content)
    {
        // 移除引用块（> 开头的行）
        var lines = content.Split('\n');
        var nonQuotedLines = lines.Where(line => !line.TrimStart().StartsWith(">"));
        return string.Join("\n", nonQuotedLines);
    }
}
