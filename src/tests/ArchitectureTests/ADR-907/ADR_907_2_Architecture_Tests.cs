using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907;

/// <summary>
/// ADR-907_2: 命名与组织规范
/// 验证 ArchitectureTests 的命名和组织规则（原 ADR-903）
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-907_2_1: 独立测试项目要求 → ADR_907_2_1_ArchitectureTests_Project_Must_Exist
/// - ADR-907_2_2: ADR 编号目录分组 → ADR_907_2_2_Test_Directory_Must_Be_Organized_By_ADR_Number
/// - ADR-907_2_3: 禁止跨 ADR 混合测试 → ADR_907_2_3_Test_Classes_Must_Map_To_Single_ADR
/// - ADR-907_2_4: 测试类命名规范 → ADR_907_2_4_Test_Class_Names_Must_Follow_Convention
/// - ADR-907_2_5: 测试方法命名规范 → ADR_907_2_5_Test_Methods_Must_Map_To_Subrules
/// - ADR-907_2_6: 失败信息溯源要求 → ADR_907_2_6_Failure_Messages_Must_Reference_ADR
/// - ADR-907_2_7: 禁止空弱断言 → ADR_907_2_7_Tests_Must_Have_Valid_Assertions
/// - ADR-907_2_8: 禁止跳过测试 → ADR_907_2_8_Tests_Must_Not_Be_Skipped
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-907.prompts.md
/// </summary>
public sealed class ADR_907_2_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests";
    private const string AdrTestsProjectPath = "src/tests/ArchitectureTests/ArchitectureTests.csproj";

    /// <summary>
    /// ADR-907_2_1: 独立测试项目要求
    /// 验证 ArchitectureTests 必须集中于独立测试项目（§2.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_1: ArchitectureTests 项目必须存在")]
    public void ADR_907_2_1_ArchitectureTests_Project_Must_Exist()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var projectPath = Path.Combine(repoRoot, AdrTestsProjectPath);

        File.Exists(projectPath).Should().BeTrue(
            $"❌ ADR-907_2_1 违规：ArchitectureTests 项目不存在\n\n" +
            $"预期路径：{projectPath}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建独立的 ArchitectureTests 测试项目\n" +
            $"  2. 项目命名格式：<SolutionName>.Tests.Architecture\n" +
            $"  3. 确保项目位于 src/tests 目录下\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.1");
    }

    /// <summary>
    /// ADR-907_2_2: ADR 编号目录分组
    /// 验证测试目录必须按 ADR 编号分组（§2.2）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_2: 测试目录必须按 ADR 编号分组")]
    public void ADR_907_2_2_Test_Directory_Must_Be_Organized_By_ADR_Number()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907_2_2 违规：ArchitectureTests 目录不存在\n\n" +
            $"预期路径：{testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ArchitectureTests 测试目录\n" +
            $"  2. 按 ADR 编号创建子目录：/ADR-XXX/\n" +
            $"  3. 每个 ADR 的测试文件放在对应子目录中\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.2");

        // 验证至少存在按 ADR 编号组织的目录
        var adrDirectories = Directory.GetDirectories(testsDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var hasAdrDirectory = adrDirectories.Length > 0 || Directory.Exists(Path.Combine(testsDirectory, "ADR"));
        
        hasAdrDirectory.Should().BeTrue(
            $"❌ ADR-907_2_2 违规：未找到按 ADR 编号组织的测试目录\n\n" +
            $"当前路径：{testsDirectory}\n" +
            $"预期格式：ADR-XXX/ 或 ADR/ADR_XXX_Architecture_Tests.cs\n\n" +
            $"修复建议：\n" +
            $"  1. 为每个 ADR 创建独立子目录：/ADR-001/, /ADR-907/ 等\n" +
            $"  2. 或使用集中式 /ADR/ 目录存放所有测试\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.2");
    }

    /// <summary>
    /// ADR-907_2_3: 禁止跨 ADR 混合测试
    /// 验证单个测试类或文件仅允许覆盖一个 ADR（§2.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_3: 测试类必须映射到单一 ADR")]
    public void ADR_907_2_3_Test_Classes_Must_Map_To_Single_ADR()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_2_3 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            
            // 测试类命名必须遵循 ADR_XXXX_Architecture_Tests.cs 格式
            if (!Regex.IsMatch(fileName, @"^ADR_\d{3,4}_Architecture_Tests\.cs$"))
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
                $"❌ ADR-907_2_3 违规：以下测试类违反单一 ADR 映射规则\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 每个测试类只能覆盖一个 ADR\n" +
                $"  2. 如果需要测试多个 ADR，创建多个测试文件\n" +
                $"  3. 测试类命名：ADR_<编号>_Architecture_Tests\n" +
                $"  4. 引用依赖的 ADR（如 ADR-900）是允许的\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.3");
        }
    }

    /// <summary>
    /// ADR-907_2_4: 测试类命名规范
    /// 验证测试类命名必须显式绑定 ADR（§2.4）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_4: 测试类命名必须遵循规范")]
    public void ADR_907_2_4_Test_Class_Names_Must_Follow_Convention()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_2_4 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs");
        var violations = new List<string>();

        // 正则表达式：ADR_<3或4位数字>_Architecture_Tests.cs
        // 推荐使用 4 位（如 ADR_0920），但也接受 3 位（如 ADR_920）
        var namingPattern = @"^ADR_\d{3,4}_Architecture_Tests\.cs$";

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            if (!Regex.IsMatch(fileName, namingPattern))
            {
                violations.Add($"  • {fileName}");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_2_4 违规：以下测试类命名不符合规范\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 测试类命名格式：ADR_<编号>_Architecture_Tests.cs\n" +
                $"  2. 推荐使用 4 位编号：ADR_0920_Architecture_Tests.cs\n" +
                $"  3. 也接受 3 位编号：ADR_920_Architecture_Tests.cs\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.4");
        }
    }

    /// <summary>
    /// ADR-907_2_5: 测试方法命名规范
    /// 验证测试方法必须映射 ADR 子规则（§2.5）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_5: 测试方法必须映射 ADR 子规则")]
    public void ADR_907_2_5_Test_Methods_Must_Map_To_Subrules()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_2_5 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
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

            // 检查方法名是否包含 ADR 子规则引用
            // 推荐格式：ADR_<编号>_<Rule>_<Clause>_<描述> 或在 DisplayName 中引用 ADR-<编号>_<Rule>_<Clause>
            foreach (var methodName in allMethods)
            {
                // 检查方法名是否以 ADR_ 开头或在 DisplayName 中引用了 ADR
                var hasAdrReference = methodName.StartsWith("ADR_") ||
                                     Regex.IsMatch(content, $@"\[(?:Fact|Theory).*?DisplayName\s*=\s*""[^""]*ADR-\d{{3,4}}[_\d]*[^""]*""[^\n]*\s+public\s+void\s+{Regex.Escape(methodName)}");

                if (!hasAdrReference)
                {
                    warnings.Add($"  • {fileName}.{methodName} - 方法名或 DisplayName 缺少 ADR 引用");
                }
            }
        }

        if (warnings.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_2_5 违规：以下测试方法缺少 ADR 子规则映射\n\n" +
                $"{string.Join("\n", warnings)}\n\n" +
                $"修复建议：\n" +
                $"  1. 推荐方法命名格式：ADR_<编号>_<Rule>_<Clause>_<描述>\n" +
                $"     示例：ADR_907_2_1_ArchitectureTests_Project_Must_Exist\n" +
                $"  2. 或在 DisplayName 中明确引用：ADR-<编号>_<Rule>_<Clause>: <描述>\n" +
                $"     示例：DisplayName = \"ADR-907_2_1: ArchitectureTests 项目必须存在\"\n" +
                $"  3. 确保每个测试方法映射到唯一的 ADR Clause\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.5");
        }
    }

    /// <summary>
    /// ADR-907_2_6: 失败信息溯源要求
    /// 验证测试失败信息必须包含 ADR 编号与子规则（§2.6）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_6: 失败信息必须包含 ADR 引用")]
    public void ADR_907_2_6_Failure_Messages_Must_Reference_ADR()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_2_6 无法执行：测试目录不存在 {testsDirectory}");
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
            
            var assertPattern = @"(Should\(\)\.(BeTrue|BeFalse)|Assert\.(True|False))\s*\(([^)]*\$?""[^""]+""(?:\s*\+\s*\$?""[^""]+"")*)\s*\)";
            var assertMatches = Regex.Matches(content, assertPattern, RegexOptions.Singleline);

            foreach (Match assertMatch in assertMatches)
            {
                // 提取断言参数部分（可能包含多个字符串连接）
                var assertArgs = assertMatch.Groups[4].Value;
                
                // 提取所有字符串字面量（支持 $"..." 和 "..."）
                var stringLiterals = Regex.Matches(assertArgs, @"\$?""([^""]+)""");
                var fullMessage = string.Join("", stringLiterals.Cast<Match>().Select(m => m.Groups[1].Value));
                
                // 检查完整消息是否包含 ADR 引用
                // 格式：❌ ADR-XXX 或 ❌ ADR-XXX_Y_Z
                var hasAdrReference = Regex.IsMatch(fullMessage, $@"ADR-0*{adrNumber}[_\d]*\s+违规");

                if (!hasAdrReference)
                {
                    violations.Add($"  • {fileName} - 断言消息缺少 ADR 引用");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_2_6 违规：以下测试文件的失败消息缺少 ADR 引用\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 所有断言失败消息必须包含 ADR 编号和 Rule/Clause\n" +
                $"  2. 推荐格式：❌ ADR-907_2_6 违规：<违规描述>\n" +
                $"  3. 包含修复建议和文档链接\n" +
                $"  4. 示例：\n" +
                $"     Assert.True(condition,\n" +
                $"         $\"❌ ADR-907_2_6 违规：失败信息必须包含 ADR 引用\\n\\n\" +\n" +
                $"         $\"修复建议：...\\n\\n\" +\n" +
                $"         $\"参考：docs/adr/governance/ADR-907-...\");\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.6");
        }
    }

    /// <summary>
    /// ADR-907_2_7: 禁止空弱断言
    /// 验证 ArchitectureTests 不得为空、占位或弱断言（§2.7）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_7: 测试不得包含空弱断言")]
    public void ADR_907_2_7_Tests_Must_Have_Valid_Assertions()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_2_7 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检测弱断言或形式化断言
            var weakPatterns = new[]
            {
                @"Assert\.True\s*\(\s*true\s*[,\)]",        // Assert.True(true)
                @"Assert\.False\s*\(\s*false\s*[,\)]",      // Assert.False(false)
                @"\.Should\(\)\.BeTrue\s*\(\s*\)\s*;",      // true.Should().BeTrue();
                @"\.Should\(\)\.BeFalse\s*\(\s*\)\s*;",     // false.Should().BeFalse();
                @"\[Fact.*?\]\s+public\s+void\s+\w+\s*\(\s*\)\s*\{\s*\}",  // 空测试方法
                @"//\s*TODO",                               // TODO 注释的占位测试
                @"throw\s+new\s+NotImplementedException"   // 未实现的测试
            };

            foreach (var pattern in weakPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.Multiline))
                {
                    violations.Add($"  • {fileName} - 包含弱断言或空测试");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_2_7 违规：以下测试包含空弱断言\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 移除所有弱断言：Assert.True(true)、Assert.False(false)\n" +
                $"  2. 移除空测试方法或 TODO 占位测试\n" +
                $"  3. 每个测试必须包含至少一个有效的架构约束验证\n" +
                $"  4. 断言必须验证实际的结构约束，不是仅验证测试可运行\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.7");
        }
    }

    /// <summary>
    /// ADR-907_2_8: 禁止跳过测试
    /// 验证不得 Skip、条件禁用测试（除非走破例机制）（§2.8）
    /// </summary>
    [Fact(DisplayName = "ADR-907_2_8: 测试不得被跳过或条件禁用")]
    public void ADR_907_2_8_Tests_Must_Not_Be_Skipped()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_2_8 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var content = File.ReadAllText(testFile);

            // 检测 Skip 或条件禁用
            var skipPatterns = new[]
            {
                @"\[Fact\s*\(\s*Skip\s*=",              // [Fact(Skip = "...")]
                @"\[Theory\s*\(\s*Skip\s*=",            // [Theory(Skip = "...")]
                @"\[SkipOnCI\]",                        // [SkipOnCI]
                @"\[Ignore\]",                          // [Ignore]
                @"#if\s+false",                         // #if false 条件编译
                @"if\s*\(\s*false\s*\)\s*{[^}]*\[Fact", // if (false) { [Fact ...] }
            };

            foreach (var pattern in skipPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    violations.Add($"  • {fileName} - 包含 Skip 或条件禁用的测试");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_2_8 违规：以下测试被跳过或条件禁用\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 移除所有 Skip、SkipOnCI、Ignore 等标记\n" +
                $"  2. 移除条件编译或条件判断来禁用测试\n" +
                $"  3. 如果测试无法通过：\n" +
                $"     a. 修复代码使测试通过（推荐）\n" +
                $"     b. 如果规则本身有问题，修订 ADR\n" +
                $"  4. ADR-907 所有规则均为 L1 级别，不允许破例\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.8");
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
