using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.13: ADR 生命周期同步校验
/// 验证 Superseded / Obsolete ADR 对应测试必须标记或移除（§4.6）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_13_Tests
{
    [Fact(DisplayName = "ADR-907.13: 废弃的 ADR 不应有活跃测试")]
    public void Superseded_ADRs_Must_Not_Have_Active_Tests()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrDocsPath);
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.13 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        // 查找所有被废弃的 ADR（在 archive 目录中或标记为 Superseded）
        var archivePath = Path.Combine(adrDirectory, "archive");
        var archivedAdrs = new HashSet<string>();

        if (Directory.Exists(archivePath))
        {
            var archivedFiles = Directory.GetFiles(archivePath, "ADR-*.md", SearchOption.AllDirectories);
            foreach (var file in archivedFiles)
            {
                var match = Regex.Match(Path.GetFileName(file), @"ADR-(\d{4})");
                if (match.Success)
                {
                    archivedAdrs.Add(match.Groups[1].Value);
                }
            }
        }

        // 检查是否有对应的测试文件
        var violations = new List<string>();
        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var match = Regex.Match(fileName, @"ADR_(\d{4})_");
            if (match.Success)
            {
                var adrNumber = match.Groups[1].Value;
                if (archivedAdrs.Contains(adrNumber))
                {
                    violations.Add($"  • {fileName} - ADR-{adrNumber} 已被归档");
                }
            }
        }

        if (violations.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.13 告警（L2）：以下测试对应的 ADR 已被废弃：",
                    ""
                }
                .Concat(violations)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 移除已废弃 ADR 的测试文件",
                    "  • 或在测试中添加明确的废弃标记",
                    "  • 更新相关文档说明测试的历史状态",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.6",
                    ""
                }));

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
        }
    }
}
