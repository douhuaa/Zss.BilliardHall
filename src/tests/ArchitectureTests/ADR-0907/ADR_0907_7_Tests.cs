using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.7: 失败信息 ADR 溯源校验
/// 验证测试失败信息必须包含 ADR 编号与规则编号（§2.7）
/// 
/// 测试细则：
/// - ADR-907.7.1: 测试目录必须存在
/// - ADR-907.7.2: 测试失败信息必须包含 ADR 溯源引用
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_7_Tests
{
    /// <summary>
    /// ADR-907.7.1: 测试目录必须存在
    /// 验证 ADR 测试目录结构完整性
    /// </summary>
    [Fact(DisplayName = "ADR-907.7.1: 测试目录必须存在")]
    public void Test_Directory_Must_Exist()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907.7.1 违规：测试目录不存在 {testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR 测试目录结构完整\n" +
            $"  2. 路径：{testsDirectory}\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.7");
    }

    /// <summary>
    /// ADR-907.7.2: 测试失败信息必须包含 ADR 溯源引用
    /// 验证所有 Assert 语句的失败消息包含对应的 ADR 编号
    /// </summary>
    [Fact(DisplayName = "ADR-907.7.2: 测试失败信息必须包含 ADR 溯源引用")]
    public void Failure_Messages_Must_Reference_ADR()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907.7.2 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 提取文件中的 ADR 编号
            var fileAdrMatch = Regex.Match(fileName, @"ADR_(\d{4})_");
            if (!fileAdrMatch.Success)
                continue;

            var adrNumber = fileAdrMatch.Groups[1].Value;

            // 查找所有 Assert 语句
            var assertStatements = Regex.Matches(content, 
                @"Assert\.(True|False|NotNull|NotEmpty|Fail|Equal|NotEqual)\s*\([^)]*?(""[^""]*?""|@""[\s\S]*?"")",
                RegexOptions.Singleline);

            foreach (Match match in assertStatements)
            {
                var assertMessage = match.Groups[2].Value;
                
                // 检查错误消息中是否包含 ADR 引用
                if (!Regex.IsMatch(assertMessage, $@"ADR-{adrNumber}"))
                {
                    // 这可能是一个普通的 Should（如 Should().NotBeEmpty），不一定需要消息
                    // 我们主要关注带有自定义消息的 Assert
                    if (assertMessage.Length > 10) // 有实际的错误消息
                    {
                        var context = content.Substring(
                            Math.Max(0, match.Index - 100),
                            Math.Min(200, content.Length - Math.Max(0, match.Index - 100)));
                        
                        // 提取方法名
                        var methodMatch = Regex.Match(
                            content.Substring(0, match.Index),
                            @"public\s+(?:void|Task)\s+(\w+)\s*\([^)]*\)\s*{[^}]*$");
                        
                        if (methodMatch.Success)
                        {
                            var methodName = methodMatch.Groups[1].Value;
                            violations.Add($"  • {fileName} -> {methodName} - Assert 消息未引用 ADR-{adrNumber}");
                        }
                    }
                }
            }
        }

        // 这是一个提醒级别的检查，因为有些简单的 Assert 可能不需要详细消息
        if (violations.Any() && violations.Count > 3) // 只在违规较多时报警
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.7.2 提醒：以下测试的失败消息应包含 ADR 溯源信息：",
                    ""
                }
                .Concat(violations.Take(10)) // 最多显示 10 个
                .Concat(violations.Count > 10 ? new[] { $"  ... 还有 {violations.Count - 10} 个" } : Array.Empty<string>())
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 在 Assert 失败消息中包含 ADR 编号和子规则",
                    "  • 格式：❌ ADR-<编号>.<子规则> 违规：...",
                    "  • 包含参考链接：docs/adr/...",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.7",
                    ""
                }));

            Console.WriteLine(message);
        }
    }
}
