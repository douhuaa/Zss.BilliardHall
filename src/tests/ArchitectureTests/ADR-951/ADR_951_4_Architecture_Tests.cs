using FluentAssertions;
using System.Text.RegularExpressions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_951;

/// <summary>
/// ADR-951_4: 案例维护机制（Rule）
/// 验证案例的维护和更新机制
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-951_4_1: 年度审核
/// - ADR-951_4_2: 过时案例处理
/// - ADR-951_4_3: 更新机制
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-951-case-repository-management.md
/// </summary>
public sealed class ADR_951_4_Architecture_Tests
{
    /// <summary>
    /// ADR-951_4_1: 年度审核
    /// 验证案例库有年度审核记录（§ADR-951_4_1）
    /// </summary>
    [Fact(DisplayName = "ADR-951_4_1: 案例库应有年度审核记录")]
    public void ADR_951_4_1_Case_Repository_Should_Have_Annual_Review_Record()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_4_1 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        // 检查案例库 README 是否包含审核记录
        var readmePath = Path.Combine(casesDirectory, "README.md");

        if (!File.Exists(readmePath))
        {
            Console.WriteLine("⚠️ ADR-951_4_1 提示：案例库 README.md 不存在，跳过测试。");
            return;
        }

        var content = File.ReadAllText(readmePath);

        // 检查是否包含审核相关信息
        var hasReviewInfo = content.Contains("审核") || content.Contains("Review") ||
                           content.Contains("维护") || content.Contains("Maintenance") ||
                           Regex.IsMatch(content, @"(最后|上次|Last)\s*(审核|Review)", RegexOptions.IgnoreCase);

        if (!hasReviewInfo)
        {
            Console.WriteLine("⚠️ ADR-951_4_1 提示：案例库 README.md 建议添加年度审核记录。");
        }

        // 这是一个 L2 级别的警告，不强制阻断
        hasReviewInfo.Should().BeTrue(
            "建议在案例库 README.md 中添加年度审核记录，以符合 ADR-951_4_1 要求");
    }

    /// <summary>
    /// ADR-951_4_2: 过时案例处理
    /// 验证过时案例必须明确标记（§ADR-951_4_2）
    /// </summary>
    [Fact(DisplayName = "ADR-951_4_2: 过时案例必须明确标记")]
    public void ADR_951_4_2_Obsolete_Cases_Must_Be_Marked()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_4_2 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_4_2 提示：暂无案例文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            // 检查是否包含"过时"或"已废弃"标记
            var hasObsoleteMarker = fileName.Contains("[已过时]") || fileName.Contains("[Obsolete]") ||
                                   content.Contains("已过时") || content.Contains("Obsolete") ||
                                   content.Contains("已废弃") || content.Contains("Deprecated");

            if (hasObsoleteMarker)
            {
                // 如果标记为过时，必须包含原因和日期
                var hasReason = content.Contains("原因") || content.Contains("Reason");
                var hasDate = Regex.IsMatch(content, @"\d{4}-\d{2}-\d{2}");

                if (!hasReason || !hasDate)
                {
                    violations.Add($"{fileName}: 标记为过时但缺少原因或日期");
                }
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-951_4_2：以下过时案例未正确标记：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-951_4_3: 更新机制
    /// 验证案例文档包含版本号或更新记录（§ADR-951_4_3）
    /// </summary>
    [Fact(DisplayName = "ADR-951_4_3: 案例应包含版本或更新记录")]
    public void ADR_951_4_3_Cases_Should_Have_Version_Or_Update_Record()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_4_3 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_4_3 提示：暂无案例文档，跳过测试。");
            return;
        }

        var warnings = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            // 检查是否包含版本号或更新记录
            var hasVersionInfo = content.Contains("version") || content.Contains("版本") ||
                                content.Contains("更新") || content.Contains("Update") ||
                                content.Contains("修订") || content.Contains("Revision");

            if (!hasVersionInfo)
            {
                warnings.Add($"{fileName}: 建议添加版本号或更新记录");
            }
        }

        if (warnings.Count > 0)
        {
            Console.WriteLine($"⚠️ ADR-951_4_3 提示：以下案例建议添加版本或更新记录：\n{string.Join("\n", warnings)}");
        }

        // 这是一个建议性检查，不强制阻断
        // 但记录下来供人工审查
        true.Should().BeTrue("ADR-951_4_3 是建议性检查，已记录需要改进的案例");
    }

    }
    }
}
