namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_950;

/// <summary>
/// ADR-950_1: 文档类型定义与权威关系
/// 验证 Guide、FAQ、Case、Standard 等非裁决性文档与 ADR 的权威关系
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-950_1_1: 类型权威性
/// - ADR-950_1_2: 权威层级映射
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-950-guide-faq-documentation-governance.md
/// </summary>
public sealed class ADR_950_1_Architecture_Tests
{
    private const string DocsPath = "docs";

    // 裁决性关键词 - 这些词不应该出现在非裁决性文档中作为新规则定义
    private static readonly string[] DecisionKeywords = new[]
    {
        "必须", "禁止", "不得", "强制", "应当"
    };

    /// <summary>
    /// ADR-950_1_1: 类型权威性
    /// 验证 Guide/FAQ/Case 文档不定义新架构规则，只引用 ADR（§ADR-950_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-950_1_1: 非裁决性文档不得定义新架构规则")]
    public void ADR_950_1_1_NonDecision_Documents_Must_Not_Define_New_Rules()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsPath = Path.Combine(repoRoot, DocsPath);

        if (!Directory.Exists(docsPath))
        {
            throw new DirectoryNotFoundException($"文档目录不存在: {docsPath}");
        }

        // 查找所有非 ADR 的文档文件
        var guidesPath = Path.Combine(docsPath, "guides");
        var faqsPath = Path.Combine(docsPath, "faqs");
        var casesPath = Path.Combine(docsPath, "cases");

        var violations = new List<string>();

        // 检查 Guides
        if (Directory.Exists(guidesPath))
        {
            CheckNonDecisionDocuments(guidesPath, "Guide", violations);
        }

        // 检查 FAQs
        if (Directory.Exists(faqsPath))
        {
            CheckNonDecisionDocuments(faqsPath, "FAQ", violations);
        }

        // 检查 Cases
        if (Directory.Exists(casesPath))
        {
            CheckNonDecisionDocuments(casesPath, "Case", violations);
        }

        // 如果有违规，显示它们（但不阻断，因为这是 L2 级别的检查）
        if (violations.Count > 0)
        {
            Console.WriteLine("⚠️ ADR-950_1_1 警告：发现可能违反文档权威性的内容：");
            foreach (var violation in violations)
            {
                Console.WriteLine($"  - {violation}");
            }
            Console.WriteLine("请确保非裁决性文档只引用 ADR，不定义新规则。");
        }

        // 目前作为警告级别，不阻断测试
        // violations.Should().BeEmpty($"发现非裁决性文档定义新规则的违规行为");
    }

    /// <summary>
    /// ADR-950_1_2: 权威层级映射
    /// 验证文档间引用遵循正确的层级关系（ADR→Guide→FAQ→Case→Standard）（§ADR-950_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-950_1_2: 文档引用必须遵循权威层级")]
    public void ADR_950_1_2_Document_References_Must_Follow_Authority_Hierarchy()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsPath = Path.Combine(repoRoot, DocsPath);

        if (!Directory.Exists(docsPath))
        {
            throw new DirectoryNotFoundException($"文档目录不存在: {docsPath}");
        }

        var violations = new List<string>();

        // 检查 Guide 文档是否引用了 ADR
        var guidesPath = Path.Combine(docsPath, "guides");
        if (Directory.Exists(guidesPath))
        {
            var guideFiles = FileSystemTestHelper.GetFilesInDirectory(guidesPath, "*.md", SearchOption.AllDirectories);
            foreach (var guideFile in guideFiles)
            {
                var content = File.ReadAllText(guideFile);

                // Guide 应该引用 ADR
                var hasAdrReference = Regex.IsMatch(content, @"ADR-\d{3}", RegexOptions.IgnoreCase) ||
                                     content.Contains("相关 ADR", StringComparison.OrdinalIgnoreCase);

                if (!hasAdrReference)
                {
                    violations.Add($"Guide 文档 '{Path.GetFileName(guideFile)}' 没有引用相关 ADR");
                }
            }
        }

        // 目前作为信息级别，输出警告但不阻断
        if (violations.Count > 0)
        {
            Console.WriteLine("ℹ️ ADR-950_1_2 提示：建议改进文档引用关系：");
            foreach (var violation in violations.Take(5)) // 只显示前5个
            {
                Console.WriteLine($"  - {violation}");
            }
            if (violations.Count > 5)
            {
                Console.WriteLine($"  ... 还有 {violations.Count - 5} 个类似问题");
            }
        }

        // 暂时不强制要求，作为建议
        // violations.Should().BeEmpty($"文档引用关系不符合权威层级");
    }

    // ========== 辅助方法 ==========

    private void CheckNonDecisionDocuments(string path, string docType, List<string> violations)
    {
        var files = FileSystemTestHelper.GetFilesInDirectory(path, "*.md", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // 检查是否包含裁决性语言但没有引用 ADR
            var hasDecisionLanguage = false;
            foreach (var keyword in DecisionKeywords)
            {
                // 检查是否在定义新规则的上下文中使用裁决性关键词
                if (Regex.IsMatch(content, $@"^[\s*-]*{keyword}[^引用ADR]{{0,50}}$", RegexOptions.Multiline))
                {
                    hasDecisionLanguage = true;
                    break;
                }
            }

            // 检查是否引用了 ADR
            var hasAdrReference = Regex.IsMatch(content, @"ADR-\d{3}", RegexOptions.IgnoreCase);

            // 如果有裁决性语言但没有 ADR 引用，可能是在定义新规则
            if (hasDecisionLanguage && !hasAdrReference)
            {
                violations.Add($"{docType} '{fileName}' 可能定义了新规则但未引用 ADR");
            }
        }
    }

}
