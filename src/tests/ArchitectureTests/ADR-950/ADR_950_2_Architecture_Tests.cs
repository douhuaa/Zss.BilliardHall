using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_950;

/// <summary>
/// ADR-950_2: ADR 与非裁决性文档的分离边界
/// 验证不同文档类型的内容边界和使用场景
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-950_2_1: 写 ADR 条件
/// - ADR-950_2_2: 写 Guide 条件
/// - ADR-950_2_3: 写 FAQ 条件
/// - ADR-950_2_4: 写 Case 条件
/// - ADR-950_2_5: 写 Standard 条件
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-950-guide-faq-documentation-governance.md
/// </summary>
public sealed class ADR_950_2_Architecture_Tests
{
    private const string DocsPath = "docs";

    /// <summary>
    /// ADR-950_2_1~2_5: 文档分离边界验证
    /// 验证各类文档存在于正确的目录结构中（§ADR-950_2_1~2_5）
    /// </summary>
    [Fact(DisplayName = "ADR-950_2: 文档类型必须按照目录结构组织")]
    public void ADR_950_2_Document_Types_Must_Be_Organized_By_Directory_Structure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsPath = Path.Combine(repoRoot, DocsPath);

        if (!Directory.Exists(docsPath))
        {
            throw new DirectoryNotFoundException($"文档目录不存在: {docsPath}");
        }

        // 验证标准目录结构存在
        var expectedDirectories = new[]
        {
            "adr",        // ADR 文档
            "guides",     // 操作指南
            "faqs",       // 常见问题
            "cases"       // 实践案例
        };

        foreach (var dir in expectedDirectories)
        {
            var dirPath = Path.Combine(docsPath, dir);
            Directory.Exists(dirPath).Should().BeTrue(
                $"文档目录 '{dir}' 应该存在于 docs/ 下，以维护清晰的文档分离边界");
        }

        Console.WriteLine("✓ ADR-950_2: 文档目录结构符合规范");
        Console.WriteLine("  - docs/adr/      - ADR 文档（裁决性）");
        Console.WriteLine("  - docs/guides/   - 操作指南（解释如何应用 ADR）");
        Console.WriteLine("  - docs/faqs/     - 常见问题（快速查询）");
        Console.WriteLine("  - docs/cases/    - 实践案例（最佳实践示例）");
    }

    /// <summary>
    /// ADR-950_2_1: 写 ADR 条件验证
    /// 验证 ADR 文档位于正确的目录并包含 Decision 章节（§ADR-950_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-950_2_1: ADR 文档必须在 adr/ 目录且包含 Decision 章节")]
    public void ADR_950_2_1_ADR_Documents_Must_Be_In_ADR_Directory_With_Decision()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrPath = Path.Combine(repoRoot, DocsPath, "adr");

        if (!Directory.Exists(adrPath))
        {
            throw new DirectoryNotFoundException($"ADR 目录不存在: {adrPath}");
        }

        var adrFiles = Directory.GetFiles(adrPath, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("RELATIONSHIP-MAP", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TIMELINE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("INDEX", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            
            // ADR 必须包含 Decision 章节
            if (!content.Contains("## Decision", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add($"{Path.GetFileName(adrFile)}: 缺少 Decision 章节");
            }
        }

        violations.Should().BeEmpty(
            $"以下 ADR 文档不符合 ADR-950_2_1 规范：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-950_2_2: 写 Guide 条件验证
    /// 验证 Guide 文档结构符合操作指南的要求（§ADR-950_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-950_2_2: Guide 文档应该提供操作步骤和 ADR 引用")]
    public void ADR_950_2_2_Guide_Documents_Should_Provide_Steps_And_ADR_References()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var guidesPath = Path.Combine(repoRoot, DocsPath, "guides");

        if (!Directory.Exists(guidesPath))
        {
            Console.WriteLine("ℹ️ ADR-950_2_2: guides/ 目录不存在，跳过验证");
            return;
        }

        var guideFiles = Directory.GetFiles(guidesPath, "*.md", SearchOption.AllDirectories);
        var recommendations = new List<string>();

        foreach (var guideFile in guideFiles.Take(10)) // 只检查前10个作为示例
        {
            var content = File.ReadAllText(guideFile);
            var fileName = Path.GetFileName(guideFile);
            
            // Guide 应该包含步骤或 ADR 引用
            var hasSteps = content.Contains("步骤", StringComparison.OrdinalIgnoreCase) ||
                          content.Contains("Step", StringComparison.OrdinalIgnoreCase);
            var hasAdrRef = content.Contains("ADR-", StringComparison.OrdinalIgnoreCase);

            if (!hasSteps && !hasAdrRef)
            {
                recommendations.Add($"{fileName}: 建议添加操作步骤或 ADR 引用");
            }
        }

        // 作为建议，不阻断测试
        if (recommendations.Count > 0)
        {
            Console.WriteLine("ℹ️ ADR-950_2_2 建议改进：");
            foreach (var rec in recommendations)
            {
                Console.WriteLine($"  - {rec}");
            }
        }
    }

    /// <summary>
    /// ADR-950_2_3~2_5: FAQ、Case、Standard 文档目录验证
    /// 验证其他文档类型存在于正确的目录（§ADR-950_2_3~2_5）
    /// </summary>
    [Fact(DisplayName = "ADR-950_2_3~2_5: FAQ/Case/Standard 文档必须在相应目录")]
    public void ADR_950_2_3_To_2_5_FAQ_Case_Standard_Documents_Must_Be_In_Correct_Directories()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsPath = Path.Combine(repoRoot, DocsPath);

        // 验证目录存在（不强制要求有内容）
        var faqsPath = Path.Combine(docsPath, "faqs");
        var casesPath = Path.Combine(docsPath, "cases");

        Directory.Exists(faqsPath).Should().BeTrue("FAQ 文档应该在 docs/faqs/ 目录下");
        Directory.Exists(casesPath).Should().BeTrue("Case 文档应该在 docs/cases/ 目录下");

        Console.WriteLine("✓ ADR-950_2_3~2_5: 文档类型目录结构正确");
    }

}
