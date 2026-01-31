using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.8: 最小断言数量与语义检测
/// 验证每个测试类至少包含 1 个有效断言（§3）
/// 
/// 测试细则：
/// - ADR-907.8.1: 测试目录必须存在
/// - ADR-907.8.2: 测试类必须包含测试方法
/// - ADR-907.8.3: 测试类必须包含断言语句
/// - ADR-907.8.4: 禁止使用 Assert.True(true) 形式化断言
/// - ADR-907.8.5: 禁止使用 Assert.False(false) 形式化断言
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_8_Tests
{
    /// <summary>
    /// ADR-907.8.1: 测试目录必须存在
    /// 验证 ADR 测试目录结构完整性
    /// </summary>
    [Fact(DisplayName = "ADR-907.8.1: 测试目录必须存在")]
    public void Test_Directory_Must_Exist()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907.8.1 违规：测试目录不存在 {testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR 测试目录结构完整\n" +
            $"  2. 路径：{testsDirectory}\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3");
    }

    /// <summary>
    /// ADR-907.8.2: 测试类必须包含测试方法
    /// 验证每个测试类都有至少一个 [Fact] 或 [Theory] 标记的测试方法
    /// </summary>
    [Fact(DisplayName = "ADR-907.8.2: 测试类必须包含测试方法")]
    public void Test_Classes_Must_Have_Test_Methods()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.8.2 无法执行：测试目录不存在 {testsDirectory}");
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
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-907.8.2 违规：以下测试类没有测试方法\n\n" +
            $"{string.Join("\n", violations)}\n\n" +
            $"修复建议：\n" +
            $"  • 每个测试类至少包含 1 个带 [Fact] 或 [Theory] 特性的测试方法\n" +
            $"  • 测试方法必须验证结构约束或 ADR 条目\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3");
    }

    /// <summary>
    /// ADR-907.8.3: 测试类必须包含断言语句
    /// 验证每个测试类都有至少一个断言语句
    /// </summary>
    [Fact(DisplayName = "ADR-907.8.3: 测试类必须包含断言语句")]
    public void Test_Classes_Must_Have_Assertions()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.8.3 无法执行：测试目录不存在 {testsDirectory}");
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
                continue; // 这个问题由 8.2 处理
            }

            // 检查是否有断言
            var hasAssertions = Regex.IsMatch(content, @"Assert\.");
            if (!hasAssertions)
            {
                violations.Add($"  • {fileName} - 没有断言");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-907.8.3 违规：以下测试类没有断言\n\n" +
            $"{string.Join("\n", violations)}\n\n" +
            $"修复建议：\n" +
            $"  • 每个测试类至少包含 1 个有效断言\n" +
            $"  • 断言必须验证结构约束或 ADR 条目\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3");
    }

    /// <summary>
    /// ADR-907.8.4: 禁止使用 Assert.True(true) 形式化断言
    /// 验证测试类不包含无意义的 Assert.True(true) 断言
    /// </summary>
    [Fact(DisplayName = "ADR-907.8.4: 禁止使用 Assert.True(true) 形式化断言")]
    public void Test_Classes_Must_Not_Use_True_True_Assertions()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.8.4 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检查是否有弱断言
            var weakAssertions = Regex.Matches(content, @"Assert\.True\s*\(\s*true\s*\)");
            if (weakAssertions.Count > 0)
            {
                violations.Add($"  • {fileName} - 包含 {weakAssertions.Count} 个形式化断言 Assert.True(true)");
            }
        }

        if (violations.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.8.4 告警（L2）：以下测试类包含形式化断言：",
                    ""
                }
                .Concat(violations)
                .Concat(new[]
                {
                    "",
                    "修复建议：",
                    "  • 禁止使用 Assert.True(true) 等形式化断言",
                    "  • 断言必须验证真实的结构约束或 ADR 条目",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3",
                    ""
                }));

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-907.8.5: 禁止使用 Assert.False(false) 形式化断言
    /// 验证测试类不包含无意义的 Assert.False(false) 断言
    /// </summary>
    [Fact(DisplayName = "ADR-907.8.5: 禁止使用 Assert.False(false) 形式化断言")]
    public void Test_Classes_Must_Not_Use_False_False_Assertions()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.8.5 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            var weakFalseAssertions = Regex.Matches(content, @"Assert\.False\s*\(\s*false\s*\)");
            if (weakFalseAssertions.Count > 0)
            {
                violations.Add($"  • {fileName} - 包含 {weakFalseAssertions.Count} 个形式化断言 Assert.False(false)");
            }
        }

        if (violations.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.8.5 告警（L2）：以下测试类包含形式化断言：",
                    ""
                }
                .Concat(violations)
                .Concat(new[]
                {
                    "",
                    "修复建议：",
                    "  • 禁止使用 Assert.False(false) 等形式化断言",
                    "  • 断言必须验证真实的结构约束或 ADR 条目",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3",
                    ""
                }));

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
        }
    }
}
