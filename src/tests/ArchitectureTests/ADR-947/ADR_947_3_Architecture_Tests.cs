using FluentAssertions;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_947;

/// <summary>
/// ADR-947_3: 禁止 ADR 编号出现在非声明语义中（Rule）
/// 验证非关系声明区禁止出现具体 ADR 编号
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-947_3_1: 编号使用约束
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md
/// </summary>
public sealed class ADR_947_3_Architecture_Tests
{
    /// <summary>
    /// ADR-947_3_1: 编号使用约束
    /// 验证非关系声明区禁止出现形如 ADR-XXXX 的具体编号，应使用占位符 ADR-#### （§ADR-947_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-947_3_1: 非关系声明区禁止出现具体 ADR 编号")]
    public void ADR_947_3_1_Non_Relationships_Section_Must_Not_Contain_Specific_ADR_Numbers()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, "docs/adr");

        var adrFiles = Directory.GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var fileName = Path.GetFileName(adrFile);
            
            // 跳过特殊文件：关系映射文档本身就是用来列出 ADR 编号的
            if (fileName.Contains("RELATIONSHIP-MAP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            var content = File.ReadAllText(adrFile);

            // 移除允许使用 ADR 编号的章节
            var contentWithoutAllowedSections = RemoveAllowedSectionsForAdrNumbers(content);

            // 查找所有 ADR-XXX 编号（排除占位符 ADR-####）
            var adrPattern = @"ADR-\d{3,4}(?!#)"; // 匹配 ADR-001 到 ADR-9999，但不匹配 ADR-####
            var matches = Regex.Matches(contentWithoutAllowedSections, adrPattern);

            if (matches.Count > 0)
            {
                var foundNumbers = matches.Select(m => m.Value).Distinct().ToList();
                // 排除 Front Matter 中的当前 ADR 编号
                var currentAdrNumber = ExtractCurrentAdrNumber(fileName);
                foundNumbers = foundNumbers.Where(n => n != currentAdrNumber).ToList();

                if (foundNumbers.Count > 0)
                {
                    violations.Add($"{fileName}: 非关系声明区包含具体 ADR 编号：{string.Join(", ", foundNumbers)}");
                }
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-947_3_1：以下 ADR 文档在非关系声明区包含具体 ADR 编号（应使用占位符 ADR-####）：\n{string.Join("\n", violations)}");
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

    private static string RemoveAllowedSectionsForAdrNumbers(string content)
    {
        // 移除 Relationships 章节
        var pattern1 = @"##\s*(Relationships|关系声明).*?\n(.*?)(?=\n##\s|\z)";
        content = Regex.Replace(content, pattern1, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 移除 References 章节
        var pattern2 = @"##\s*(References|参考|非裁决性参考).*?\n(.*?)(?=\n##\s|\z)";
        content = Regex.Replace(content, pattern2, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 移除 History 章节（版本历史中经常引用 ADR 编号）
        var pattern3 = @"##\s*(History|版本历史|变更历史).*?\n(.*?)(?=\n##\s|\z)";
        content = Regex.Replace(content, pattern3, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return content;
    }

    private static string ExtractCurrentAdrNumber(string fileName)
    {
        var match = Regex.Match(fileName, @"ADR-\d{3,4}");
        return match.Success ? match.Value : string.Empty;
    }
}
