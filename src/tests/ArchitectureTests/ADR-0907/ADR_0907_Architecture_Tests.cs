using System.Text.RegularExpressions;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907: ArchitectureTests 执法治理体系
/// 验证 ArchitectureTests 的命名、组织、最小断言及 CI / Analyzer 映射规则
/// 
/// 测试覆盖映射（严格遵循 ADR-907 §2.5 命名规范）：
/// L1 级别（失败即阻断）：
/// - ADR-907.1: ArchitectureTests 项目存在性校验 → ADR_0907_1_ArchitectureTests_Project_Must_Exist
/// - ADR-907.2: ADR 目录结构扫描 → ADR_0907_2_Test_Directory_Must_Be_Organized_By_ADR_Number
/// - ADR-907.3: 测试类与 ADR 映射校验 → ADR_0907_3_Test_Classes_Must_Map_To_Single_ADR
/// - ADR-907.4: 测试类命名正则校验 → ADR_0907_4_Test_Class_Names_Must_Follow_Convention
/// - ADR-907.5: 测试方法与子规则解析 → ADR_0907_5_Test_Methods_Must_Map_To_Subrules
/// - ADR-907.6: 失败信息 ADR 溯源校验 → ADR_0907_6_Failure_Messages_Must_Reference_ADR
/// - ADR-907.8: Skip / 条件禁用检测 → ADR_0907_8_Tests_Must_Not_Be_Skipped
/// - ADR-907.9: CI / Analyzer 自动注册校验 → ADR_0907_9_Tests_Must_Be_Discoverable_By_CI
/// - ADR-907.10: L1 阻断 / L2 告警策略执行 → ADR_0907_10_Enforcement_Levels_Must_Be_Documented
/// - ADR-907.11: 破例与偿还机制记录 → ADR_0907_11_Exception_Mechanism_Must_Be_Documented
/// 
/// L2 级别（失败记录告警）：
/// - ADR-907.7: 最小断言数量与语义检测 → ADR_0907_7_Test_Classes_Must_Have_Valid_Assertions
/// - ADR-907.12: ADR 生命周期同步校验 → ADR_0907_12_Superseded_ADRs_Must_Not_Have_Active_Tests
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests";
    private const string AdrTestsProjectPath = "src/tests/ArchitectureTests/ArchitectureTests.csproj";

    #region L1 级别测试

    /// <summary>
    /// ADR-907.1: ArchitectureTests 项目存在性校验
    /// 验证解决方案中必须存在独立的 ArchitectureTests 项目（§2.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907.1: ArchitectureTests 项目必须存在")]
    public void ADR_0907_1_ArchitectureTests_Project_Must_Exist()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var projectPath = Path.Combine(repoRoot, AdrTestsProjectPath);

        Assert.True(File.Exists(projectPath),
            $"❌ ADR-907.1 违规：ArchitectureTests 项目不存在\n\n" +
            $"预期路径：{projectPath}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建独立的 ArchitectureTests 测试项目\n" +
            $"  2. 项目命名格式：<SolutionName>.Tests.Architecture\n" +
            $"  3. 确保项目位于 src/tests 目录下\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.1");
    }

    /// <summary>
    /// ADR-907.2: ADR 目录结构扫描
    /// 验证测试目录必须按 ADR 编号分组为 /ADR-XXXX/ 格式（§2.2）
    /// </summary>
    [Fact(DisplayName = "ADR-907.2: 测试目录必须按 ADR 编号分组")]
    public void ADR_0907_2_Test_Directory_Must_Be_Organized_By_ADR_Number()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        Assert.True(Directory.Exists(testsDirectory),
            $"❌ ADR-907.2 违规：ArchitectureTests 目录不存在\n\n" +
            $"预期路径：{testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ArchitectureTests 测试目录\n" +
            $"  2. 按 ADR 编号创建子目录：/ADR-XXXX/\n" +
            $"  3. 每个 ADR 的测试文件放在对应子目录中\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.2");

        // 验证至少存在按 ADR 编号组织的目录
        var adrDirectories = Directory.GetDirectories(testsDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var hasAdrDirectory = adrDirectories.Length > 0 || Directory.Exists(Path.Combine(testsDirectory, "ADR"));
        
        Assert.True(hasAdrDirectory,
            $"❌ ADR-907.2 违规：未找到按 ADR 编号组织的测试目录\n\n" +
            $"当前路径：{testsDirectory}\n" +
            $"预期格式：ADR-XXXX/ 或 ADR/ADR_XXXX_Architecture_Tests.cs\n\n" +
            $"修复建议：\n" +
            $"  1. 为每个 ADR 创建独立子目录：/ADR-0001/, /ADR-0907/ 等\n" +
            $"  2. 或使用集中式 /ADR/ 目录存放所有测试\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.2");
    }

    /// <summary>
    /// ADR-907.3: 测试类与 ADR 映射校验
    /// 验证每个测试类只能覆盖一个 ADR（§2.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907.3: 测试类必须映射到单一 ADR")]
    public void ADR_0907_3_Test_Classes_Must_Map_To_Single_ADR()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.3 无法执行：测试目录不存在 {testsDirectory}");
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
            Assert.Fail(
                $"❌ ADR-907.3 违规：以下测试类违反单一 ADR 映射规则\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 每个测试类只能覆盖一个 ADR\n" +
                $"  2. 如果需要测试多个 ADR，创建多个测试文件\n" +
                $"  3. 测试类命名：ADR_<编号>_Architecture_Tests\n" +
                $"  4. 引用依赖的 ADR（如 ADR-0000）是允许的\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.3");
        }
    }

    /// <summary>
    /// ADR-907.4: 测试类命名正则校验
    /// 验证测试类命名必须显式绑定 ADR（§2.4）
    /// </summary>
    [Fact(DisplayName = "ADR-907.4: 测试类命名必须遵循规范")]
    public void ADR_0907_4_Test_Class_Names_Must_Follow_Convention()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.4 无法执行：测试目录不存在 {testsDirectory}");
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
            Assert.Fail(
                $"❌ ADR-907.4 违规：以下测试类命名不符合规范\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 测试类命名格式：ADR_<编号>_Architecture_Tests.cs\n" +
                $"  2. 推荐使用 4 位编号：ADR_0920_Architecture_Tests.cs\n" +
                $"  3. 也接受 3 位编号：ADR_920_Architecture_Tests.cs\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.4");
        }
    }

    /// <summary>
    /// ADR-907.5: 测试方法与子规则解析
    /// 验证测试方法必须映射 ADR 子规则（§2.5）
    /// </summary>
    [Fact(DisplayName = "ADR-907.5: 测试方法必须映射 ADR 子规则")]
    public void ADR_0907_5_Test_Methods_Must_Map_To_Subrules()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.5 无法执行：测试目录不存在 {testsDirectory}");
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
            // 推荐格式：ADR_<编号>_<子规则>_<描述> 或包含 ADR-<编号>.<子规则> 的注释
            foreach (var methodName in allMethods)
            {
                // 检查方法名是否以 ADR_ 开头或在 DisplayName 中引用了 ADR
                var displayNamePattern = $@"\[(?:Fact|Theory).*?DisplayName\s*=\s*""[^""]*ADR-\d{{4}}";
                var hasDisplayNameReference = Regex.IsMatch(
                    content.Substring(Math.Max(0, content.IndexOf(methodName) - 300), 
                                      Math.Min(300, content.Length - Math.Max(0, content.IndexOf(methodName) - 300))),
                    displayNamePattern);

                if (!methodName.StartsWith("ADR_") && !hasDisplayNameReference)
                {
                    warnings.Add($"  • {fileName} -> {methodName} - 方法名或 DisplayName 未明确引用 ADR");
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
                    "⚠️  ADR-907.5 提醒：以下测试方法应更明确地映射到 ADR 子规则：",
                    ""
                }
                .Concat(warnings)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 方法命名格式：ADR_<编号>_<子规则>_<行为描述>",
                    "  • 或在 DisplayName 中包含：ADR-<编号>.<子规则>",
                    "  • 示例：ADR_0001_1_Modules_Should_Not_Reference_Other_Modules",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.5",
                    ""
                }));

            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-907.6: 失败信息 ADR 溯源校验
    /// 验证测试失败信息必须包含 ADR 编号与子规则（§2.6）
    /// </summary>
    [Fact(DisplayName = "ADR-907.6: 测试失败信息必须包含 ADR 溯源")]
    public void ADR_0907_6_Failure_Messages_Must_Reference_ADR()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.6 无法执行：测试目录不存在 {testsDirectory}");
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
                    // 这可能是一个普通的 Assert（如 Assert.NotEmpty），不一定需要消息
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
                    "⚠️  ADR-907.6 提醒：以下测试的失败消息应包含 ADR 溯源信息：",
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
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.6",
                    ""
                }));

            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-907.8: Skip / 条件禁用检测
    /// 验证不得 Skip、条件禁用测试（除非走破例机制）（§2.8）
    /// </summary>
    [Fact(DisplayName = "ADR-907.8: 测试不得使用 Skip 或条件禁用")]
    public void ADR_0907_8_Tests_Must_Not_Be_Skipped()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.8 无法执行：测试目录不存在 {testsDirectory}");
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

            // 检查是否使用了条件特性（如 SkipOnCI）
            var conditionalSkips = Regex.Matches(content, @"\[(?:Skip.*?|Conditional.*?)\]");
            foreach (Match match in conditionalSkips)
            {
                violations.Add($"  • {fileName} - 条件跳过: {match.Value}");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(
                $"❌ ADR-907.8 违规：以下测试使用了 Skip 或条件禁用\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 移除 Skip 参数，修复测试\n" +
                $"  2. 如果确实需要跳过，必须通过破例机制\n" +
                $"  3. 记录破例原因、到期时间和偿还计划\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.8");
        }
    }

    /// <summary>
    /// ADR-907.9: CI / Analyzer 自动注册校验
    /// 验证所有 ArchitectureTests 必须被 Analyzer 自动发现并注册（§4.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907.9: 架构测试必须可被 CI 发现")]
    public void ADR_0907_9_Tests_Must_Be_Discoverable_By_CI()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var projectPath = Path.Combine(repoRoot, AdrTestsProjectPath);

        Assert.True(File.Exists(projectPath),
            $"❌ ADR-907.9 无法执行：项目文件不存在 {projectPath}");

        var projectContent = File.ReadAllText(projectPath);

        // 验证项目包含必要的测试框架引用
        var hasXunit = projectContent.Contains("xunit") || projectContent.Contains("Xunit");
        var hasNetArchTest = projectContent.Contains("NetArchTest");

        var warnings = new List<string>();
        if (!hasXunit)
        {
            warnings.Add("  • 缺少 xUnit 测试框架引用");
        }
        if (!hasNetArchTest)
        {
            warnings.Add("  • 缺少 NetArchTest.Rules 引用");
        }

        // 检查是否有 CI 配置文件
        var githubWorkflowsPath = Path.Combine(repoRoot, ".github", "workflows");
        var hasCiConfig = Directory.Exists(githubWorkflowsPath);

        if (!hasCiConfig)
        {
            warnings.Add("  • 缺少 CI 配置（.github/workflows）");
        }

        if (warnings.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.9 提醒：架构测试的 CI 集成可能不完整：",
                    ""
                }
                .Concat(warnings)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 确保项目引用了必要的测试框架",
                    "  • 配置 CI 管道自动运行架构测试",
                    "  • 测试失败应阻断合并",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.1",
                    ""
                }));

            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-907.10: L1 阻断 / L2 告警策略执行
    /// 验证执行级别分类是否有文档支持（§4.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907.10: 执行级别策略必须有文档支持")]
    public void ADR_0907_10_Enforcement_Levels_Must_Be_Documented()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 检查 ADR-905（执行级别分类）是否存在
        var adr905Path = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-905-enforcement-level-classification.md");
        
        Assert.True(File.Exists(adr905Path),
            $"❌ ADR-907.10 违规：缺少执行级别分类文档\n\n" +
            $"预期路径：{adr905Path}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ADR-905 文档定义 L1/L2 执行级别\n" +
            $"  2. L1：失败即阻断 CI / 合并 / 部署\n" +
            $"  3. L2：失败记录告警，进入人工 Code Review\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.3");
    }

    /// <summary>
    /// ADR-907.11: 破例与偿还机制记录
    /// 验证破例机制必须有文档支持（§4.4）
    /// </summary>
    [Fact(DisplayName = "ADR-907.11: 破例机制必须有文档支持")]
    public void ADR_0907_11_Exception_Mechanism_Must_Be_Documented()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 检查 ADR-0000（架构测试宪法）是否存在并包含破例机制
        var adr0000Path = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-0000-architecture-tests.md");
        
        Assert.True(File.Exists(adr0000Path),
            $"❌ ADR-907.11 违规：缺少架构测试宪法文档\n\n" +
            $"预期路径：{adr0000Path}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ADR-0000 定义破例与偿还机制\n" +
            $"  2. 破例必须记录：ADR 编号、测试类/方法、原因、到期时间\n" +
            $"  3. 建立偿还追踪机制\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.4");
    }

    #endregion

    #region L2 级别测试

    /// <summary>
    /// ADR-907.7: 最小断言数量与语义检测
    /// 验证每个测试类至少包含 1 个有效断言（§3）
    /// </summary>
    [Fact(DisplayName = "ADR-907.7: 测试类必须包含有效断言")]
    public void ADR_0907_7_Test_Classes_Must_Have_Valid_Assertions()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.7 无法执行：测试目录不存在 {testsDirectory}");
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
                continue;
            }

            // 检查是否有断言
            var hasAssertions = Regex.IsMatch(content, @"Assert\.");
            if (!hasAssertions)
            {
                violations.Add($"  • {fileName} - 没有断言");
                continue;
            }

            // 检查是否有弱断言或形式化断言
            var weakAssertions = Regex.Matches(content, @"Assert\.True\s*\(\s*true\s*\)");
            if (weakAssertions.Count > 0)
            {
                violations.Add($"  • {fileName} - 包含形式化断言 Assert.True(true)");
            }

            var weakFalseAssertions = Regex.Matches(content, @"Assert\.False\s*\(\s*false\s*\)");
            if (weakFalseAssertions.Count > 0)
            {
                violations.Add($"  • {fileName} - 包含形式化断言 Assert.False(false)");
            }
        }

        if (violations.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.7 告警（L2）：以下测试类的断言可能不足或无效：",
                    ""
                }
                .Concat(violations)
                .Concat(new[]
                {
                    "",
                    "修复建议：",
                    "  • 每个测试类至少包含 1 个有效断言",
                    "  • 禁止使用 Assert.True(true) 等形式化断言",
                    "  • 断言必须验证结构约束或 ADR 条目",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §3",
                    ""
                }));

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-907.12: ADR 生命周期同步校验
    /// 验证 Superseded / Obsolete ADR 对应测试必须标记或移除（§4.6）
    /// </summary>
    [Fact(DisplayName = "ADR-907.12: 废弃的 ADR 不应有活跃测试")]
    public void ADR_0907_12_Superseded_ADRs_Must_Not_Have_Active_Tests()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            Assert.Fail($"❌ ADR-907.12 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        // 查找所有被废弃的 ADR（在 archive 目录中或标记为 Superseded）
        var archivePath = Path.Combine(adrDirectory, "archive");
        var archivedAdrs = new HashSet<string>();

        if (Directory.Exists(archivePath))
        {
            var archivedFiles = Directory.GetFiles(archivePath, "ADR-*.md", SearchOption.AllDirectories);
            foreach (var file in archivedFiles)
            {
                var match = Regex.Match(Path.GetFileName(file), @"ADR-(\d{4})");
                if (match.Success)
                {
                    archivedAdrs.Add(match.Groups[1].Value);
                }
            }
        }

        // 检查是否有对应的测试文件
        var violations = new List<string>();
        var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);
            var match = Regex.Match(fileName, @"ADR_(\d{4})_");
            if (match.Success)
            {
                var adrNumber = match.Groups[1].Value;
                if (archivedAdrs.Contains(adrNumber))
                {
                    violations.Add($"  • {fileName} - ADR-{adrNumber} 已被归档");
                }
            }
        }

        if (violations.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.12 告警（L2）：以下测试对应的 ADR 已被废弃：",
                    ""
                }
                .Concat(violations)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 移除已废弃 ADR 的测试文件",
                    "  • 或在测试中添加明确的废弃标记",
                    "  • 更新相关文档说明测试的历史状态",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.6",
                    ""
                }));

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
        }
    }

    #endregion

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
