using FluentAssertions;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_951;

/// <summary>
/// ADR-951_3: 案例审核标准（Rule）
/// 验证案例的审核流程和标准
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-951_3_1: Core 案例审核流程
/// - ADR-951_3_2: Reference 案例审核流程
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-951-case-repository-management.md
/// </summary>
public sealed class ADR_951_3_Architecture_Tests
{
    /// <summary>
    /// ADR-951_3_1: Core 案例审核流程
    /// 验证 Core 级别案例必须通过完整审核流程（§ADR-951_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-951_3_1: Core 案例必须通过完整审核流程")]
    public void ADR_951_3_1_Core_Cases_Must_Pass_Full_Review_Process()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_3_1 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_3_1 提示：暂无案例文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            // 检查是否为 Core 级别案例
            var isCoreCase = Regex.IsMatch(content, @"(级别|Level)\s*[:：]\s*Core", RegexOptions.IgnoreCase);

            if (!isCoreCase)
            {
                continue; // 不是 Core 案例，跳过
            }

            // Core 案例必须包含审核记录
            var hasReviewRecord = content.Contains("审核") || content.Contains("Review") || 
                                 content.Contains("reviewer") || content.Contains("审查");

            if (!hasReviewRecord)
            {
                violations.Add($"{fileName}: Core 级别案例缺少审核记录");
            }

            // 检查是否有 ADR 合规性声明
            var hasAdrCompliance = Regex.IsMatch(content, @"ADR-\d{3,4}");

            if (!hasAdrCompliance)
            {
                violations.Add($"{fileName}: Core 级别案例缺少 ADR 引用或合规性声明");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-951_3_1：以下 Core 级别案例未通过完整审核流程：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-951_3_2: Reference 案例审核流程
    /// 验证 Reference 级别案例必须通过基础审核流程（§ADR-951_3_2）
    /// </summary>
    [Fact(DisplayName = "ADR-951_3_2: Reference 案例必须通过基础审核流程")]
    public void ADR_951_3_2_Reference_Cases_Must_Pass_Basic_Review_Process()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_3_2 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_3_2 提示：暂无案例文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            // 检查是否为 Reference 级别案例
            var isReferenceCase = Regex.IsMatch(content, @"(级别|Level)\s*[:：]\s*Reference", RegexOptions.IgnoreCase);

            if (!isReferenceCase)
            {
                continue; // 不是 Reference 案例，跳过
            }

            // Reference 案例至少需要有作者或日期信息
            var hasAuthorOrDate = content.Contains("作者") || content.Contains("Author") ||
                                 content.Contains("日期") || content.Contains("Date") ||
                                 Regex.IsMatch(content, @"\d{4}-\d{2}-\d{2}");

            if (!hasAuthorOrDate)
            {
                violations.Add($"{fileName}: Reference 级别案例缺少作者或日期信息");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-951_3_2：以下 Reference 级别案例未满足基础审核要求：\n{string.Join("\n", violations)}");
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
