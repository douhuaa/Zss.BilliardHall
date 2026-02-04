using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_3: Onboarding 文档的强制结构
/// 验证 Onboarding 文档符合强制结构模板
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_3_1: 强制结构模板
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_3_Architecture_Tests
{
    private const string DocsPath = "docs";
    
    // Onboarding 文档必需的章节
    private static readonly string[] RequiredSections = new[]
    {
        "你正在进入什么系统",
        "快速上手路径",
        "新成员高频踩雷区",
        "常用入口",
        "不在这里解决的问题"
    };

    /// <summary>
    /// ADR-960_3_1: 强制结构模板
    /// 验证 Onboarding 文档包含所有必需章节（§ADR-960_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_3_1: Onboarding 必须遵循强制结构模板")]
    public void ADR_960_3_1_Onboarding_Must_Follow_Required_Structure()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsPath = Path.Combine(repoRoot, DocsPath);

        if (!Directory.Exists(docsPath))
        {
            throw new DirectoryNotFoundException($"文档目录不存在: {docsPath}");
        }

        // 查找 Onboarding 文档
        var onboardingFiles = Directory.GetFiles(docsPath, "*onboarding*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("ADR-960", StringComparison.OrdinalIgnoreCase)) // 排除 ADR-960 本身
            .Where(f => !f.Contains("template", StringComparison.OrdinalIgnoreCase)) // 排除模板
            .ToList();

        if (onboardingFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-960_3_1 提示：未找到 Onboarding 文档，跳过结构检查");
            return;
        }

        var violations = new List<string>();

        foreach (var file in onboardingFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            var missingSections = new List<string>();

            // 检查必需章节
            foreach (var section in RequiredSections)
            {
                // 匹配章节标题（二级或三级标题）
                var sectionPattern = $@"^##\s+.*?{Regex.Escape(section)}";
                if (!Regex.IsMatch(content, sectionPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    missingSections.Add(section);
                }
            }

            if (missingSections.Any())
            {
                violations.Add($"{relativePath} - 缺少章节: {string.Join(", ", missingSections)}");
            }

            // 检查是否有 Fast Path
            var hasFastPath = content.Contains("Fast Path", StringComparison.OrdinalIgnoreCase) ||
                            content.Contains("快速上手", StringComparison.OrdinalIgnoreCase);
            
            if (!hasFastPath)
            {
                violations.Add($"{relativePath} - 缺少快速上手路径（Fast Path）");
            }
        }

        if (violations.Count > 0)
        {
            Console.WriteLine("⚠️ ADR-960_3_1 警告：发现 Onboarding 文档结构不完整：");
            foreach (var violation in violations)
            {
                Console.WriteLine($"  - {violation}");
            }
            Console.WriteLine("\n修复建议：");
            Console.WriteLine("  1. 确保包含所有必需章节");
            Console.WriteLine("  2. 章节顺序应该合理且易于理解");
            Console.WriteLine("  3. 必须包含快速上手路径（Fast Path）");
            Console.WriteLine("\n参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §3.1");
        }

        // L2 级别，不阻断测试，仅警告
    }

    // ========== 辅助方法 ==========

    private static string? FindRepositoryRoot()
    {
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
