using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907;

/// <summary>
/// ADR-907_3: 最小断言语义规范
/// 验证 ArchitectureTests 的断言要求（原 ADR-904）
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-907_3_1: 最小断言数量要求 → ADR_907_3_1_Test_Classes_Must_Have_Minimum_Assertions
/// - ADR-907_3_2: 单一子规则映射 → ADR_907_3_2_Test_Methods_Must_Map_To_Single_Subrule
/// - ADR-907_3_3: 失败信息可溯源性 → ADR_907_3_3_Failure_Messages_Must_Be_Traceable
/// - ADR-907_3_4: 禁止形式化断言 → ADR_907_3_4_Formal_Assertions_Are_Prohibited
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-907.prompts.md
/// </summary>
public sealed class ADR_907_3_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests";

    /// <summary>
    /// ADR-907_3_1: 最小断言数量要求
    /// 验证每个测试类至少包含 1 个有效断言（§3.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907_3_1: 测试类必须包含至少一个有效断言")]
    public void ADR_907_3_1_Test_Classes_Must_Have_Minimum_Assertions()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_3_1 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 统计有效断言数量
            var assertPatterns = new[]
            {
                @"Assert\.True\s*\(",
                @"Assert\.False\s*\(",
                @"Assert\.Equal\s*\(",
                @"Assert\.NotEqual\s*\(",
                @"\.Should\(\)\.BeTrue\s*\(",
                @"\.Should\(\)\.BeFalse\s*\(",
                @"\.Should\(\)\.Be\s*\(",
                @"\.Should\(\)\.NotBe\s*\(",
                @"\.Should\(\)\.BeEmpty\s*\(",
                @"\.Should\(\)\.NotBeEmpty\s*\(",
                @"\.Should\(\)\.NotBeNull\s*\(",
                @"\.Should\(\)\.Contain\s*\(",
                @"\.Should\(\)\.NotContain\s*\(",
                @"\.Should\(\)\.StartWith\s*\(",
                @"\.Should\(\)\.BeGreaterThan\s*\(",
                @"\.Should\(\)\.BeGreaterThanOrEqualTo\s*\(",
                @"\.Should\(\)\.BeLessThan\s*\(",
                @"\.Should\(\)\.BeLessThanOrEqualTo\s*\(",
                @"\.Should\(\)\.NotBeNullOrEmpty\s*\(",
            };

            var assertCount = assertPatterns.Sum(pattern => 
                Regex.Matches(content, pattern).Count);

            // 排除形式化断言
            var formalAssertions = new[]
            {
                @"Assert\.True\s*\(\s*true\s*[,\)]",
                @"Assert\.False\s*\(\s*false\s*[,\)]",
            };

            var formalCount = formalAssertions.Sum(pattern => 
                Regex.Matches(content, pattern).Count);

            var effectiveAssertCount = assertCount - formalCount;

            if (effectiveAssertCount < 1)
            {
                violations.Add($"  • {fileName} - 有效断言数量: {effectiveAssertCount} (至少需要 1 个)");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_3_1 违规：以下测试类缺少有效断言\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 每个测试类必须包含至少 1 个有效断言\n" +
                $"  2. 断言必须验证架构约束，不是形式化断言\n" +
                $"  3. 有效断言示例：\n" +
                $"     - Assert.True(File.Exists(path), \"文件必须存在\")\n" +
                $"     - result.IsSuccessful.Should().BeTrue(\"规则必须通过\")\n" +
                $"  4. 无效断言示例：\n" +
                $"     - Assert.True(true)\n" +
                $"     - Assert.False(false)\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3.1");
        }
    }

    /// <summary>
    /// ADR-907_3_2: 单一子规则映射
    /// 验证每个测试方法只能映射一个 ADR 子规则（§3.2）
    /// </summary>
    [Fact(DisplayName = "ADR-907_3_2: 测试方法必须映射到单一子规则")]
    public void ADR_907_3_2_Test_Methods_Must_Map_To_Single_Subrule()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_3_2 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 查找所有测试方法
            var factMethods = Regex.Matches(content, @"\[Fact.*?DisplayName\s*=\s*""([^""]+)""[^\]]*\]\s+public\s+void\s+(\w+)");
            var theoryMethods = Regex.Matches(content, @"\[Theory.*?DisplayName\s*=\s*""([^""]+)""[^\]]*\]\s+public\s+void\s+(\w+)");

            var allMethods = factMethods.Cast<Match>()
                .Concat(theoryMethods.Cast<Match>())
                .ToList();

            foreach (var method in allMethods)
            {
                var displayName = method.Groups[1].Value;
                var methodName = method.Groups[2].Value;

                // 检查 DisplayName 中是否引用了多个 ADR 子规则
                // 格式：ADR-907_1_1, ADR-907_1_2 等
                var subruleMatches = Regex.Matches(displayName, @"ADR-\d{3,4}_\d+_\d+");
                
                if (subruleMatches.Count > 1)
                {
                    violations.Add($"  • {fileName}.{methodName} - DisplayName 引用了多个子规则");
                }

                // 检查方法名中是否包含多个子规则引用
                // 格式：ADR_907_1_1, ADR_907_1_2 等
                var methodSubrules = Regex.Matches(methodName, @"ADR_\d{3,4}_\d+_\d+");
                
                if (methodSubrules.Count > 1)
                {
                    violations.Add($"  • {fileName}.{methodName} - 方法名包含多个子规则引用");
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_3_2 违规：以下测试方法违反单一子规则映射\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 每个测试方法只能映射一个 ADR 子规则（Clause）\n" +
                $"  2. 如果需要测试多个子规则，创建多个测试方法\n" +
                $"  3. 方法命名格式：ADR_<编号>_<Rule>_<Clause>_<描述>\n" +
                $"     示例：ADR_907_3_2_Test_Methods_Must_Map_To_Single_Subrule\n" +
                $"  4. DisplayName 格式：ADR-<编号>_<Rule>_<Clause>: <描述>\n" +
                $"     示例：\"ADR-907_3_2: 测试方法必须映射到单一子规则\"\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3.2");
        }
    }

    /// <summary>
    /// ADR-907_3_3: 失败信息可溯源性
    /// 验证所有断言失败信息必须可反向溯源到 ADR（§3.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907_3_3: 失败信息必须可溯源到 ADR")]
    public void ADR_907_3_3_Failure_Messages_Must_Be_Traceable()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_3_3 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 提取文件中的 ADR 编号
            var fileAdrMatch = Regex.Match(fileName, @"ADR_(\d{3,4})_");
            if (!fileAdrMatch.Success)
                continue;

            var adrNumber = fileAdrMatch.Groups[1].Value;

            // 查找所有断言语句及其完整消息（包括多行字符串连接）
            // 支持字符串插值 ($"...") 和普通字符串 ("...")
            // 支持多行字符串连接：.BeTrue($"part1" + $"part2" + ...)
            // 支持所有常用的 FluentAssertions API 方法
            
            var assertPattern = @"(Should\(\)\.(BeTrue|BeFalse|Be|NotBe|BeEmpty|NotBeEmpty|NotBeNull|Contain|NotContain|StartWith|BeGreaterThan|BeGreaterThanOrEqualTo|BeLessThan|BeLessThanOrEqualTo|NotBeNullOrEmpty)|Assert\.(True|False|Equal|NotEqual))\s*\(([^)]*\$?""[^""]+""(?:\s*\+\s*\$?""[^""]+"")*)\s*\)";
            var assertMatches = Regex.Matches(content, assertPattern, RegexOptions.Singleline);

            foreach (Match assertMatch in assertMatches)
            {
                // 提取断言参数部分（可能包含多个字符串连接）
                // 由于扩展了断言方法，最后一个捕获组索引需要动态获取
                var assertArgs = assertMatch.Groups[assertMatch.Groups.Count - 1].Value;

                // 提取所有字符串字面量（支持 $"..." 和 "..."）
                var stringLiterals = Regex.Matches(assertArgs, @"\$?""([^""]+)""");
                var fullMessage = string.Join("", stringLiterals.Cast<Match>().Select(m => m.Groups[1].Value));

                // 检查失败消息的完整性
                var hasAdrReference = Regex.IsMatch(fullMessage, $@"ADR-0*{adrNumber}[_\d]*");
                var hasViolationMarker = fullMessage.Contains("违规") || fullMessage.Contains("violation");
                var hasFixSuggestion = fullMessage.Contains("修复建议") || fullMessage.Contains("fix");
                var hasDocReference = fullMessage.Contains("参考：docs/adr") || fullMessage.Contains("reference:");

                if (!hasAdrReference)
                {
                    violations.Add($"  • {fileName} - 断言消息缺少 ADR 引用");
                    break;
                }

                if (!hasViolationMarker)
                {
                    violations.Add($"  • {fileName} - 断言消息缺少违规标记（❌ 或 violation）");
                    break;
                }

                if (!hasFixSuggestion)
                {
                    violations.Add($"  • {fileName} - 断言消息缺少修复建议");
                    break;
                }

                if (!hasDocReference)
                {
                    violations.Add($"  • {fileName} - 断言消息缺少文档引用");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_3_3 违规：以下测试的失败信息不完整\n\n" +
                $"{string.Join("\n", violations.Distinct())}\n\n" +
                $"修复建议：\n" +
                $"  1. 所有断言失败消息必须包含以下要素：\n" +
                $"     a. ADR 编号和 Rule/Clause（如 ADR-907_3_3）\n" +
                $"     b. 违规标记（❌）\n" +
                $"     c. 违规描述\n" +
                $"     d. 修复建议（具体步骤）\n" +
                $"     e. 文档引用（参考：docs/adr/...）\n" +
                $"  2. 完整示例：\n" +
                $"     Assert.True(condition,\n" +
                $"         $\"❌ ADR-907_3_3 违规：失败信息必须可溯源到 ADR\\n\\n\" +\n" +
                $"         $\"违规详情：...\\n\\n\" +\n" +
                $"         $\"修复建议：\\n\" +\n" +
                $"         $\"  1. 步骤一\\n\" +\n" +
                $"         $\"  2. 步骤二\\n\\n\" +\n" +
                $"         $\"参考：docs/adr/governance/ADR-907-...\");\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3.3");
        }
    }

    /// <summary>
    /// ADR-907_3_4: 禁止形式化断言
    /// 验证明确禁止 Assert.True(true) 等形式化断言（§3.4）
    /// </summary>
    [Fact(DisplayName = "ADR-907_3_4: 禁止形式化断言")]
    public void ADR_907_3_4_Formal_Assertions_Are_Prohibited()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_3_4 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检测形式化断言
            var formalPatterns = new Dictionary<string, string>
            {
                [@"Assert\.True\s*\(\s*true\s*[,\)]"] = "Assert.True(true)",
                [@"Assert\.False\s*\(\s*false\s*[,\)]"] = "Assert.False(false)",
                [@"true\.Should\(\)\.BeTrue\s*\(\s*\)"] = "true.Should().BeTrue()",
                [@"false\.Should\(\)\.BeFalse\s*\(\s*\)"] = "false.Should().BeFalse()",
                [@"1\.Should\(\)\.Be\s*\(\s*1\s*\)"] = "Tautological assertion",
            };

            foreach (var pattern in formalPatterns)
            {
                if (Regex.IsMatch(content, pattern.Key, RegexOptions.Multiline))
                {
                    violations.Add($"  • {fileName} - 包含形式化断言：{pattern.Value}");
                }
            }

            // 检测仅验证测试可运行的断言
            var runOnlyPatterns = new[]
            {
                @"//\s*This\s+test\s+just\s+ensures\s+the\s+code\s+runs",
                @"//\s*仅验证测试可运行",
                @"//\s*No\s+actual\s+assertion",
            };

            foreach (var pattern in runOnlyPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"  • {fileName} - 包含仅验证可运行的注释");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_3_4 违规：以下测试包含形式化断言\n\n" +
                $"{string.Join("\n", violations.Distinct())}\n\n" +
                $"修复建议：\n" +
                $"  1. 禁止以下形式化断言：\n" +
                $"     ❌ Assert.True(true)\n" +
                $"     ❌ Assert.False(false)\n" +
                $"     ❌ true.Should().BeTrue()\n" +
                $"     ❌ 1.Should().Be(1)\n" +
                $"  2. 断言必须验证实际的结构约束\n" +
                $"  3. 正确示例：\n" +
                $"     ✅ File.Exists(path).Should().BeTrue(\"项目文件必须存在\")\n" +
                $"     ✅ result.IsSuccessful.Should().BeTrue(\"架构规则必须通过\")\n" +
                $"     ✅ violations.Should().BeEmpty(\"不应有违规\")\n" +
                $"  4. 测试不应仅验证代码可运行，必须验证架构约束\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3.4");
        }
    }

    #region Helper Methods

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, AdrDocsPath)))
            {
                return currentDir;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return null;
    }

    #endregion
}
