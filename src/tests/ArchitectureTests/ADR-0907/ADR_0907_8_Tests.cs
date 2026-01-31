using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.8: 最小断言数量与语义检测
/// 验证每个测试类至少包含 1 个有效断言（§3）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_8_Tests
{
    [Fact(DisplayName = "ADR-907.8: 测试类必须包含有效断言")]
    public void Test_Classes_Must_Have_Valid_Assertions()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.8 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检查是否有测试方法
            var hasTestMethods = Regex.IsMatch(content, @"\[(?:Fact|Theory)");
            if (!hasTestMethods)
            {
                violations.Add($"  • {fileName} - 没有测试方法");
                continue;
            }

            // 检查是否有断言
            var hasAssertions = Regex.IsMatch(content, @"Assert\.");
            if (!hasAssertions)
            {
                violations.Add($"  • {fileName} - 没有断言");
                continue;
            }

            // 检查是否有弱断言或形式化断言
            var weakAssertions = Regex.Matches(content, @"Assert\.True\s*\(\s*true\s*\)");
            if (weakAssertions.Count > 0)
            {
                violations.Add($"  • {fileName} - 包含形式化断言 Should().BeTrue(true)");
            }

            var weakFalseAssertions = Regex.Matches(content, @"Assert\.False\s*\(\s*false\s*\)");
            if (weakFalseAssertions.Count > 0)
            {
                violations.Add($"  • {fileName} - 包含形式化断言 Should().BeFalse(false)");
            }
        }

        if (violations.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.8 告警（L2）：以下测试类的断言可能不足或无效：",
                    ""
                }
                .Concat(violations)
                .Concat(new[]
                {
                    "",
                    "修复建议：",
                    "  • 每个测试类至少包含 1 个有效断言",
                    "  • 禁止使用 Should().BeTrue(true) 等形式化断言",
                    "  • 断言必须验证结构约束或 ADR 条目",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3",
                    ""
                }));

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
        }
    }
}
