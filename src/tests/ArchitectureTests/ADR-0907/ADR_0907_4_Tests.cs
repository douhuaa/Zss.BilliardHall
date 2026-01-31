using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.4: 测试类与 ADR 映射校验
/// 验证每个测试类只能覆盖一个 ADR（§2.4）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_4_Tests
{
    [Fact(DisplayName = "ADR-907.4: 测试类必须映射到单一 ADR")]
    public void Test_Classes_Must_Map_To_Single_ADR()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.4 无法执行：测试目录不存在 {testsDirectory}");
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
                continue;
            }

            // 验证文件内容主要测试单一 ADR
            var content = File.ReadAllText(testFile);
            
            // 提取文件名中的 ADR 编号
            var fileAdrMatch = Regex.Match(fileName, @"ADR_(\d{3,4})_");
            if (!fileAdrMatch.Success)
                continue;

            var fileAdr = fileAdrMatch.Groups[1].Value;
            
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
                $"❌ ADR-907.4 违规：以下测试类违反单一 ADR 映射规则\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 每个测试类只能覆盖一个 ADR\n" +
                $"  2. 如果需要测试多个 ADR，创建多个测试文件\n" +
                $"  3. 测试类命名：ADR_<编号>_<规则编号>_Tests 或 ADR_<编号>_Architecture_Tests\n" +
                $"  4. 引用依赖的 ADR（如 ADR-0000）是允许的\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.4");
        }
    }
}
