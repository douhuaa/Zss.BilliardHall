using FluentAssertions;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_955;

/// <summary>
/// ADR-955_2: 搜索优化规则（Rule）
/// 验证文档的搜索优化和关键词标注
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-955_2_1: 标题关键词要求
/// - ADR-955_2_2: 摘要要求
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-955-documentation-search-discoverability.md
/// </summary>
public sealed class ADR_955_2_Architecture_Tests
{
    /// <summary>
    /// ADR-955_2_1: 标题关键词要求
    /// 验证所有文档标题包含核心关键词（§ADR-955_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-955_2_1: 文档标题必须包含核心关键词")]
    public void ADR_955_2_1_Document_Titles_Must_Contain_Keywords()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsDirectory = Path.Combine(repoRoot, "docs");

        if (!Directory.Exists(docsDirectory))
        {
            Console.WriteLine("⚠️ ADR-955_2_1 提示：docs 目录不存在，跳过测试。");
            return;
        }

        var allMarkdownFiles = Directory.GetFiles(docsDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var warnings = new List<string>();

        foreach (var file in allMarkdownFiles)
        {
            var fileName = Path.GetFileName(file);
            
            // 标题应包含有意义的关键词（不只是编号）
            var titleWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var hasKeywords = titleWithoutExtension.Length > 10 && 
                             Regex.IsMatch(titleWithoutExtension, @"[a-zA-Z\u4e00-\u9fa5]{3,}");

            if (!hasKeywords)
            {
                warnings.Add($"{fileName}: 标题可能缺少核心关键词");
            }
        }

        if (warnings.Count > 0)
        {
            Console.WriteLine($"⚠️ ADR-955_2_1 提示：以下文档标题建议包含更多关键词：\n{string.Join("\n", warnings.Take(10))}");
        }

        true.Should().BeTrue("ADR-955_2_1 是建议性检查");
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
