using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.9: Skip / 条件禁用检测
/// 验证不得 Skip、条件禁用测试（除非走破例机制）（§2.9）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_9_Tests
{
    [Fact(DisplayName = "ADR-907.9: 测试不得使用 Skip 或条件禁用")]
    public void Tests_Must_Not_Be_Skipped()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.9 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检查是否使用了 Skip 参数
            var skipMatches = Regex.Matches(content, @"\[(?:Fact|Theory).*?Skip\s*=\s*""([^""]+)""");
            foreach (Match match in skipMatches)
            {
                var skipReason = match.Groups[1].Value;
                violations.Add($"  • {fileName} - Skip: {skipReason}");
            }

            // 检查是否使用了条件特性（如 SkipOnCI）
            var conditionalSkips = Regex.Matches(content, @"\[(?:Skip.*?|Conditional.*?)\]");
            foreach (Match match in conditionalSkips)
            {
                violations.Add($"  • {fileName} - 条件跳过: {match.Value}");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907.9 违规：以下测试使用了 Skip 或条件禁用\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 移除 Skip 参数，修复测试\n" +
                $"  2. 如果确实需要跳过，必须通过破例机制\n" +
                $"  3. 记录破例原因、到期时间和偿还计划\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.9");
        }
    }
}
