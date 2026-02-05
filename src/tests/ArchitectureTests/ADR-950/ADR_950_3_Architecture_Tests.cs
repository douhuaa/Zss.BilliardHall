using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_950;

/// <summary>
/// ADR-950_3: 文档结构标准
/// 验证 Guide、FAQ、Case 等文档符合标准结构
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-950_3_1: Guide 标准结构
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-950-guide-faq-documentation-governance.md
/// </summary>
public sealed class ADR_950_3_Architecture_Tests
{
    private const string DocsPath = "docs";

    /// <summary>
    /// ADR-950_3_1: Guide 标准结构
    /// 验证 Guide 文档包含必要的结构元素（§ADR-950_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-950_3_1: Guide 文档应包含标准结构元素")]
    public void ADR_950_3_1_Guide_Documents_Should_Contain_Standard_Structure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var guidesPath = Path.Combine(repoRoot, DocsPath, "guides");

        if (!Directory.Exists(guidesPath))
        {
            Console.WriteLine("ℹ️ ADR-950_3_1: guides/ 目录不存在，跳过验证");
            return;
        }

        var guideFiles = Directory.GetFiles(guidesPath, "*.md", SearchOption.AllDirectories);
        
        if (guideFiles.Length == 0)
        {
            Console.WriteLine("ℹ️ ADR-950_3_1: 没有找到 Guide 文档，跳过验证");
            return;
        }

        var recommendations = new List<string>();

        // 标准 Guide 结构元素（根据 ADR-950_3_1）
        var standardElements = new[]
        {
            new { Pattern = @"目的[:：]", Name = "目的" },
            new { Pattern = @"相关\s*ADR[:：]", Name = "相关 ADR" },
            new { Pattern = @"前置条件[:：]", Name = "前置条件" },
            new { Pattern = @"适用场景[:：]", Name = "适用场景" }
        };

        foreach (var guideFile in guideFiles.Take(10)) // 检查前10个作为示例
        {
            var content = File.ReadAllText(guideFile);
            var fileName = Path.GetFileName(guideFile);
            var missingElements = new List<string>();

            foreach (var element in standardElements)
            {
                if (!Regex.IsMatch(content, element.Pattern, RegexOptions.IgnoreCase))
                {
                    missingElements.Add(element.Name);
                }
            }

            if (missingElements.Count > 0)
            {
                recommendations.Add($"{fileName}: 建议添加 {string.Join("、", missingElements)}");
            }
        }

        // 作为建议，不阻断测试（L2 级别）
        if (recommendations.Count > 0)
        {
            Console.WriteLine("ℹ️ ADR-950_3_1 建议：以下 Guide 文档可以改进结构：");
            foreach (var rec in recommendations.Take(5))
            {
                Console.WriteLine($"  - {rec}");
            }
            if (recommendations.Count > 5)
            {
                Console.WriteLine($"  ... 还有 {recommendations.Count - 5} 个文档可以改进");
            }
            Console.WriteLine("\n标准 Guide 结构应包含：");
            Console.WriteLine("  1. 目的：一句话说明");
            Console.WriteLine("  2. 相关 ADR：ADR-XXXX 链接");
            Console.WriteLine("  3. 前置条件：需要了解的内容");
            Console.WriteLine("  4. 适用场景：何时使用本指南");
        }
        else
        {
            Console.WriteLine("✓ ADR-950_3_1: Guide 文档结构符合标准");
        }
    }

}
