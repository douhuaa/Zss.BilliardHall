using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.9: Skip / 条件禁用检测
/// 验证不得 Skip、条件禁用测试（除非走破例机制）（§2.9）
/// 
/// 测试细则：
/// - ADR-907.9.1: 测试目录必须存在
/// - ADR-907.9.2: 测试不得使用 Skip 参数
/// - ADR-907.9.3: 测试不得使用条件跳过特性
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_9_Tests
{
    /// <summary>
    /// ADR-907.9.1: 测试目录必须存在
    /// 验证 ADR 测试目录结构完整性
    /// </summary>
    [Fact(DisplayName = "ADR-907.9.1: 测试目录必须存在")]
    public void Test_Directory_Must_Exist()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907.9.1 违规：测试目录不存在 {testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR 测试目录结构完整\n" +
            $"  2. 路径：{testsDirectory}\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.9");
    }

    /// <summary>
    /// ADR-907.9.2: 测试不得使用 Skip 参数
    /// 验证测试特性中不存在 Skip 参数，确保所有测试都被执行
    /// </summary>
    [Fact(DisplayName = "ADR-907.9.2: 测试不得使用 Skip 参数")]
    public void Tests_Must_Not_Use_Skip_Parameter()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.9.2 无法执行：测试目录不存在 {testsDirectory}");
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
        }

        violations.Should().BeEmpty(
            $"❌ ADR-907.9.2 违规：以下测试使用了 Skip 参数\n\n" +
            $"{string.Join("\n", violations)}\n\n" +
            $"修复建议：\n" +
            $"  1. 移除 Skip 参数，修复测试\n" +
            $"  2. 如果确实需要跳过，必须通过破例机制\n" +
            $"  3. 记录破例原因、到期时间和偿还计划\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.9");
    }

    /// <summary>
    /// ADR-907.9.3: 测试不得使用条件跳过特性
    /// 验证测试类不使用条件跳过特性（如 SkipOnCI、Conditional 等）
    /// </summary>
    [Fact(DisplayName = "ADR-907.9.3: 测试不得使用条件跳过特性")]
    public void Tests_Must_Not_Use_Conditional_Skip_Attributes()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.9.3 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检查是否使用了条件特性（如 SkipOnCI）
            var conditionalSkips = Regex.Matches(content, @"\[(?:Skip.*?|Conditional.*?)\]");
            foreach (Match match in conditionalSkips)
            {
                violations.Add($"  • {fileName} - 条件跳过: {match.Value}");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-907.9.3 违规：以下测试使用了条件禁用特性\n\n" +
            $"{string.Join("\n", violations)}\n\n" +
            $"修复建议：\n" +
            $"  1. 移除条件跳过特性，修复测试\n" +
            $"  2. 如果确实需要跳过，必须通过破例机制\n" +
            $"  3. 记录破例原因、到期时间和偿还计划\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.9");
    }
}
