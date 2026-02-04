using FluentAssertions;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_947;

/// <summary>
/// ADR-947_4: 禁止同编号多文档（Rule）
/// 验证同一 ADR 编号只能对应一个文件
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-947_4_1: 文件唯一性
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md
/// </summary>
public sealed class ADR_947_4_Architecture_Tests
{
    /// <summary>
    /// ADR-947_4_1: 文件唯一性
    /// 验证同一 ADR 编号只能对应一个文件，不允许 ADR-005-A.md 和 ADR-005-B.md 同时存在（§ADR-947_4_1）
    /// </summary>
    [Fact(DisplayName = "ADR-947_4_1: 同一 ADR 编号只能对应一个文件")]
    public void ADR_947_4_1_Same_ADR_Number_Must_Have_Only_One_File()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, "docs/adr");

        var adrFiles = Directory.GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories);

        // 提取 ADR 编号（仅数字部分）
        var adrNumberGroups = adrFiles
            .Select(file => new
            {
                FilePath = file,
                FileName = Path.GetFileName(file),
                AdrNumber = ExtractAdrNumber(Path.GetFileName(file))
            })
            .Where(x => !string.IsNullOrEmpty(x.AdrNumber))
            .GroupBy(x => x.AdrNumber)
            .ToList();

        var violations = new List<string>();

        foreach (var group in adrNumberGroups)
        {
            if (group.Count() > 1)
            {
                var files = string.Join(", ", group.Select(x => x.FileName));
                violations.Add($"ADR-{group.Key}: 存在多个文件 ({files})");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-947_4_1：以下 ADR 编号对应多个文件：\n{string.Join("\n", violations)}");
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

    private static string ExtractAdrNumber(string fileName)
    {
        // 提取 ADR-XXX 中的 XXX 数字部分（不包括后缀如 -A, -alignment-checklist 等）
        var match = Regex.Match(fileName, @"^ADR-(\d{3,4})(?:-|\.md)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
