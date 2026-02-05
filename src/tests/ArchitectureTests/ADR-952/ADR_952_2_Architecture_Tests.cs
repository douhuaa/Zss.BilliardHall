using FluentAssertions;
using System.Text.RegularExpressions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_952;

/// <summary>
/// ADR-952_2: 工程标准必须基于 ADR（Rule）
/// 验证工程标准必须明确声明基于的 ADR 并仅细化实施细节
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-952_2_1: 明确声明基于的 ADR
/// - ADR-952_2_2: 仅细化 ADR 的实施细节
/// - ADR-952_2_3: 不得引入 ADR 未授权的新约束
/// - ADR-952_2_4: Standard 文档必需章节
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-952-engineering-standard-adr-boundary.md
/// </summary>
public sealed class ADR_952_2_Architecture_Tests
{
    /// <summary>
    /// ADR-952_2_1: 明确声明基于的 ADR
    /// 验证工程标准文档必须明确声明基于的 ADR（§ADR-952_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-952_2_1: 工程标准必须明确声明基于的 ADR")]
    public void ADR_952_2_1_Standards_Must_Declare_Based_ADR()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var standardsDirectory = Path.Combine(repoRoot, "docs/engineering-standards");

        if (!Directory.Exists(standardsDirectory))
        {
            Console.WriteLine("⚠️ ADR-952_2_1 提示：docs/engineering-standards/ 目录尚未创建，跳过测试。");
            return;
        }

        var standardFiles = Directory.GetFiles(standardsDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("Best-Practice")) // 排除 Best Practice
            .ToList();

        if (standardFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-952_2_1 提示：暂无工程标准文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var standardFile in standardFiles)
        {
            var fileName = Path.GetFileName(standardFile);
            var content = File.ReadAllText(standardFile);

            // 检查是否明确声明基于的 ADR
            var hasAdrReference = Regex.IsMatch(content, 
                @"(基于|Based\s+on)\s*ADR-\d{3,4}", 
                RegexOptions.IgnoreCase);

            if (!hasAdrReference)
            {
                violations.Add($"{fileName}: 未明确声明基于的 ADR");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-952_2_1：以下工程标准文档未明确声明基于的 ADR：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-952_2_4: Standard 文档必需章节
    /// 验证工程标准文档包含必需的章节结构（§ADR-952_2_4）
    /// </summary>
    [Fact(DisplayName = "ADR-952_2_4: 工程标准文档必须包含必需章节")]
    public void ADR_952_2_4_Standards_Must_Have_Required_Sections()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var standardsDirectory = Path.Combine(repoRoot, "docs/engineering-standards");

        if (!Directory.Exists(standardsDirectory))
        {
            Console.WriteLine("⚠️ ADR-952_2_4 提示：docs/engineering-standards/ 目录尚未创建，跳过测试。");
            return;
        }

        var standardFiles = Directory.GetFiles(standardsDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("Best-Practice"))
            .ToList();

        if (standardFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-952_2_4 提示：暂无工程标准文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var standardFile in standardFiles)
        {
            var fileName = Path.GetFileName(standardFile);
            var content = File.ReadAllText(standardFile);

            var missingElements = new List<string>();

            // 检查必需元素
            if (!Regex.IsMatch(content, @"(基于|Based\s+on)\s*ADR", RegexOptions.IgnoreCase))
            {
                missingElements.Add("基于 ADR");
            }

            if (!content.Contains("类型") && !content.Contains("Type"))
            {
                missingElements.Add("类型");
            }

            if (!content.Contains("级别") && !content.Contains("Level"))
            {
                missingElements.Add("强制级别");
            }

            if (missingElements.Count > 0)
            {
                violations.Add($"{fileName}: 缺少必需元素 - {string.Join(", ", missingElements)}");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-952_2_4：以下工程标准文档缺少必需章节：\n{string.Join("\n", violations)}");
    }

    }
    }
}
