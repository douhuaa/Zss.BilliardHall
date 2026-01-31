using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.5: 测试类命名正则校验（支持规则编号）
/// 验证测试类命名必须显式绑定 ADR 和规则编号（§2.5）
/// 
/// 测试细则：
/// - ADR-907.5.1: 测试目录必须存在
/// - ADR-907.5.2: 测试类命名必须符合命名规范
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_5_Tests
{
    /// <summary>
    /// ADR-907.5.1: 测试目录必须存在
    /// 验证 ADR 测试目录结构完整性
    /// </summary>
    [Fact(DisplayName = "ADR-907.5.1: 测试目录必须存在")]
    public void Test_Directory_Must_Exist()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907.5.1 违规：测试目录不存在 {testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR 测试目录结构完整\n" +
            $"  2. 路径：{testsDirectory}\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.5");
    }

    /// <summary>
    /// ADR-907.5.2: 测试类命名必须符合命名规范
    /// 验证所有测试类遵循 ADR_<编号>_<规则编号>_Tests.cs 或 ADR_<编号>_Architecture_Tests.cs 格式
    /// </summary>
    [Fact(DisplayName = "ADR-907.5.2: 测试类命名必须符合命名规范")]
    public void Test_Class_Names_Must_Follow_Naming_Convention()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.5.2 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs");
        var violations = new List<string>();

        // 正则表达式支持两种格式：
        // 1. ADR_<编号>_<规则编号>_Tests.cs（推荐，避免类膨胀）
        // 2. ADR_<编号>_Architecture_Tests.cs（兼容格式）
        var namingPattern = @"^ADR_\d{3,4}_(\d+_Tests|Architecture_Tests)\.cs$";

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            if (!Regex.IsMatch(fileName, namingPattern))
            {
                violations.Add($"  • {fileName}");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-907.5.2 违规：以下测试类命名不符合规范\n\n" +
            $"{string.Join("\n", violations)}\n\n" +
            $"修复建议：\n" +
            $"  1. 推荐格式（规则独立）：ADR_<编号>_<规则编号>_Tests.cs\n" +
            $"     示例：ADR_0907_1_Tests.cs、ADR_0907_2_Tests.cs\n" +
            $"  2. 兼容格式：ADR_<编号>_Architecture_Tests.cs\n" +
            $"     示例：ADR_0907_Architecture_Tests.cs\n" +
            $"  3. 推荐使用 4 位 ADR 编号\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.5");
    }
}
