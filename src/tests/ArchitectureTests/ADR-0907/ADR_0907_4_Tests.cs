using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.4: 测试类与 ADR 映射校验
/// 验证每个测试类只能覆盖一个 ADR（§2.4）
/// 
/// 测试细则：
/// - 细则 1: 测试类命名必须符合规范格式
/// - 细则 2: 测试类DisplayName 必须引用正确的 ADR
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_4_Tests
{
    /// <summary>
    /// 细则 1: 验证测试类命名格式符合规范
    /// </summary>
    [Fact(DisplayName = "ADR-907.4.1: 测试类命名必须符合规范格式")]
    public void Test_Class_Names_Must_Follow_Naming_Convention()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.4.1 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            
            // 测试类命名必须遵循以下格式之一：
            // 1. ADR_XXXX_Y_Tests.cs（规则独立测试，推荐）
            // 2. ADR_XXXX_Architecture_Tests.cs（兼容格式）
            if (!Regex.IsMatch(fileName, @"^ADR_\d{3,4}_(\d+_Tests|Architecture_Tests)\.cs$"))
            {
                violations.Add($"  • {fileName} - 命名格式不符合规范");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907.4.1 违规：以下测试类命名格式不符合规范\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 测试类命名：ADR_<编号>_<规则编号>_Tests 或 ADR_<编号>_Architecture_Tests\n" +
                $"  2. 推荐使用规则独立格式：ADR_0907_1_Tests.cs\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.4");
        }
    }

    /// <summary>
    /// 细则 2: 验证测试类 DisplayName 引用正确的 ADR
    /// </summary>
    [Fact(DisplayName = "ADR-907.4.2: DisplayName 必须引用正确的 ADR 编号")]
    public void DisplayName_Must_Reference_Correct_ADR()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.4.2 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            
            // 提取文件名中的 ADR 编号
            var fileAdrMatch = Regex.Match(fileName, @"ADR_(\d{3,4})_");
            if (!fileAdrMatch.Success)
                continue;

            var fileAdr = fileAdrMatch.Groups[1].Value;
            
            // 验证文件内容主要测试单一 ADR
            var content = File.ReadAllText(testFile);
            
            // 查找测试方法的 DisplayName
            var displayNames = Regex.Matches(content, @"DisplayName\s*=\s*""([^""]+)""");
            
            foreach (Match match in displayNames)
            {
                var displayName = match.Groups[1].Value;
                // 检查 DisplayName 是否明确测试其他 ADR（不是依赖引用）
                var adrMatch = Regex.Match(displayName, @"ADR-(\d{3,4})");
                if (adrMatch.Success)
                {
                    var testAdr = adrMatch.Groups[1].Value.PadLeft(4, '0');
                    var normalizedFileAdr = fileAdr.PadLeft(4, '0');
                    if (testAdr != normalizedFileAdr)
                    {
                        violations.Add($"  • {fileName} - DisplayName 测试了不同的 ADR: ADR-{testAdr}");
                        break;
                    }
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907.4.2 违规：以下测试类 DisplayName 引用了不同的 ADR\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 每个测试类只能覆盖一个 ADR\n" +
                $"  2. DisplayName 必须引用文件名中的 ADR 编号\n" +
                $"  3. 如果需要测试多个 ADR，创建多个测试文件\n" +
                $"  4. 引用依赖的 ADR（如 ADR-0000）是允许的\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.4");
        }
    }
}
