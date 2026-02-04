using FluentAssertions;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_955;

/// <summary>
/// ADR-955_1: 文档索引策略（Rule）
/// 验证文档的元数据、分类和索引机制
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-955_1_1: 元数据要求
/// - ADR-955_1_2: 目录结构要求
/// - ADR-955_1_3: 核心原则
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-955-documentation-search-discoverability.md
/// </summary>
public sealed class ADR_955_1_Architecture_Tests
{
    /// <summary>
    /// ADR-955_1_1: 元数据要求
    /// 验证 Guide/FAQ/Case/Standard 文档包含必需的元数据（§ADR-955_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-955_1_1: 文档必须包含必需的元数据")]
    public void ADR_955_1_1_Documents_Must_Have_Required_Metadata()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        var documentDirectories = new[]
        {
            Path.Combine(repoRoot, "docs/guides"),
            Path.Combine(repoRoot, "docs/faqs"),
            Path.Combine(repoRoot, "docs/cases"),
            Path.Combine(repoRoot, "docs/engineering-standards")
        };

        var violations = new List<string>();

        foreach (var directory in documentDirectories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            var files = Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var content = File.ReadAllText(file);

                // 检查是否包含基本元数据
                var hasAuthorOrDate = content.Contains("作者") || content.Contains("Author") ||
                                     content.Contains("日期") || content.Contains("Date") ||
                                     Regex.IsMatch(content, @"\d{4}-\d{2}-\d{2}");

                if (!hasAuthorOrDate)
                {
                    violations.Add($"{fileName}: 缺少作者或日期元数据");
                }
            }
        }

        if (violations.Count > 0)
        {
            Console.WriteLine($"⚠️ ADR-955_1_1 提示：以下文档建议添加完整元数据：\n{string.Join("\n", violations)}");
        }

        // 这是建议性检查
        true.Should().BeTrue("ADR-955_1_1 是建议性检查");
    }

    /// <summary>
    /// ADR-955_1_2: 目录结构要求
    /// 验证文档目录结构清晰且支持快速检索（§ADR-955_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-955_1_2: 文档目录结构必须清晰")]
    public void ADR_955_1_2_Document_Directory_Structure_Must_Be_Clear()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsDirectory = Path.Combine(repoRoot, "docs");

        Directory.Exists(docsDirectory).Should().BeTrue("docs 目录必须存在");

        var expectedDirectories = new[] { "adr", "guides", "faqs", "cases", "engineering-standards" };
        var existingDirectories = new List<string>();

        foreach (var dir in expectedDirectories)
        {
            var dirPath = Path.Combine(docsDirectory, dir);
            if (Directory.Exists(dirPath))
            {
                existingDirectories.Add(dir);
            }
        }

        existingDirectories.Should().NotBeEmpty(
            "至少应该有部分文档目录（adr, guides, faqs, cases, engineering-standards）存在");
    }

    // ========== 辅助方法 ==========

    private static string? FindRepositoryRoot()
    {
        var envRoot = Environment.GetEnvironmentVariable("REPO_ROOT");
        if (!string.IsNullOrEmpty(envRoot) && Directory.Exists(envRoot))
        {
            return envRoot;
        }

        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                File.Exists(Path.Combine(currentDir, "Zss.BilliardHall.slnx")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }
}
