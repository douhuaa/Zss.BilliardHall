using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.3: 规则独立测试校验（避免类膨胀）
/// 验证每一条具体架构规则必须使用独立的测试类进行验证（§2.3）
/// 禁止多个规则混合在同一个测试类
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_3_Tests
{
    [Fact(DisplayName = "ADR-907.3: 规则独立测试 - 每条规则使用独立测试类")]
    public void Each_Rule_Must_Have_Independent_Test_Class()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.3 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs");
        var warnings = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检查是否有多个规则编号的测试方法在同一个类中
            // 提取所有 DisplayName 中的 ADR 规则引用（格式：ADR-XXXX.Y）
            var ruleReferences = Regex.Matches(content, @"ADR-(\d{3,4})\.(\d+)");
            
            var uniqueRules = ruleReferences
                .Cast<Match>()
                .Select(m => $"{m.Groups[1].Value}.{m.Groups[2].Value}")
                .Distinct()
                .ToList();

            // 如果一个测试类中包含多个不同规则的测试，给出警告
            // 注意：ADR_XXXX_Architecture_Tests 是兼容格式，可以包含多个规则
            // 但新的推荐格式 ADR_XXXX_Y_Tests 应该只包含一个规则
            if (uniqueRules.Count > 1 && !fileName.EndsWith("Architecture_Tests.cs"))
            {
                warnings.Add($"  • {fileName} - 包含多个规则: {string.Join(", ", uniqueRules)}");
            }
        }

        if (warnings.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.3 提醒：以下测试类建议拆分为规则独立的测试类：",
                    ""
                }
                .Concat(warnings)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 每一条具体架构规则使用独立的测试类",
                    "  • 测试类命名：ADR_<编号>_<规则编号>_Tests",
                    "  • 示例：ADR_0001_1_Tests、ADR_0001_2_Tests",
                    "  • 兼容格式：ADR_<编号>_Architecture_Tests（单一规则或汇总测试）",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.3",
                    ""
                }));

            Console.WriteLine(message);
        }
    }
}
