using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.6: 测试方法映射行为场景校验
/// 验证测试方法必须映射规则下的具体行为场景（§2.6）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_6_Tests
{
    [Fact(DisplayName = "ADR-907.6: 测试方法必须映射行为场景")]
    public void Test_Methods_Must_Map_To_Behavior_Scenarios()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.6 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs")
            .Where(f => Path.GetFileName(f).StartsWith("ADR_"))
            .ToArray();
        var warnings = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 查找所有测试方法（标记了 [Fact] 或 [Theory]）
            var factMethods = Regex.Matches(content, @"\[Fact.*?\]\s+public\s+void\s+(\w+)\s*\(");
            var theoryMethods = Regex.Matches(content, @"\[Theory.*?\]\s+public\s+void\s+(\w+)\s*\(");

            var allMethods = factMethods.Cast<Match>()
                .Concat(theoryMethods.Cast<Match>())
                .Select(m => m.Groups[1].Value)
                .ToList();

            // 检查方法名是否在 DisplayName 中引用了 ADR 和规则编号
            foreach (var methodName in allMethods)
            {
                var displayNamePattern = $@"\[(?:Fact|Theory).*?DisplayName\s*=\s*""[^""]*ADR-\d{{3,4}}\.\d+";
                var hasDisplayNameReference = Regex.IsMatch(
                    content.Substring(Math.Max(0, content.IndexOf(methodName) - 300), 
                                      Math.Min(300, content.Length - Math.Max(0, content.IndexOf(methodName) - 300))),
                    displayNamePattern);

                if (!hasDisplayNameReference)
                {
                    warnings.Add($"  • {fileName} -> {methodName} - DisplayName 未引用 ADR 和规则编号");
                }
            }
        }

        // 这是一个提醒级别的检查
        if (warnings.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.6 提醒：以下测试方法应在 DisplayName 中明确映射 ADR 和规则：",
                    ""
                }
                .Concat(warnings)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 测试方法对应规则下的具体行为场景",
                    "  • 在 DisplayName 中包含：ADR-<编号>.<规则编号>: <场景描述>",
                    "  • 示例：[Fact(DisplayName = \"ADR-0001.1: 模块不能直接引用其他模块\")]",
                    "  • 方法名应描述具体场景，无需重复 ADR 编号",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.6",
                    ""
                }));

            Console.WriteLine(message);
        }
    }
}
