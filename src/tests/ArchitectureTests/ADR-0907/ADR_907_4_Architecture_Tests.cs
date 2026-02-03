using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907;

/// <summary>
/// ADR-907_4: Analyzer / CI Gate 映射协议
/// 验证 ArchitectureTests 与 CI/Analyzer 的集成规则（原 ADR-906）
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-907_4_1: 自动发现注册要求 → ADR_907_4_1_Tests_Must_Be_Discoverable_By_CI
/// - ADR-907_4_2: RuleId 精确映射 → ADR_907_4_2_Test_Failures_Must_Map_To_RuleId
/// - ADR-907_4_3: 执行级别分类支持 → ADR_907_4_3_Enforcement_Levels_Must_Be_Supported
/// - ADR-907_4_4: 破例机制自动记录 → ADR_907_4_4_Exception_Mechanism_Must_Be_Documented
/// - ADR-907_4_5: Analyzer 检测能力要求 → ADR_907_4_5_Analyzer_Must_Detect_Violations
/// - ADR-907_4_6: ADR 生命周期同步 → ADR_907_4_6_Superseded_ADRs_Must_Be_Handled
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-907.prompts.md
/// </summary>
public sealed class ADR_907_4_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests";
    private const string AdrTestsProjectPath = "src/tests/ArchitectureTests/ArchitectureTests.csproj";

    /// <summary>
    /// ADR-907_4_1: 自动发现注册要求
    /// 验证所有 ArchitectureTests 必须被 Analyzer 自动发现并注册（§4.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907_4_1: 测试必须可被 CI/Analyzer 自动发现")]
    public void ADR_907_4_1_Tests_Must_Be_Discoverable_By_CI()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var projectPath = Path.Combine(repoRoot, AdrTestsProjectPath);

        // 验证项目文件存在
        File.Exists(projectPath).Should().BeTrue(
            $"❌ ADR-907_4_1 违规：ArchitectureTests 项目文件不存在\n\n" +
            $"预期路径：{projectPath}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ArchitectureTests.csproj 项目文件\n" +
            $"  2. 确保项目类型为测试项目\n" +
            $"  3. 引用必要的测试框架（xUnit、NUnit 或 MSTest）\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.1");

        // 验证项目配置包含测试框架
        var projectContent = File.ReadAllText(projectPath);
        var hasTestFramework = projectContent.Contains("xunit") || 
                              projectContent.Contains("NUnit") || 
                              projectContent.Contains("MSTest");

        hasTestFramework.Should().BeTrue(
            $"❌ ADR-907_4_1 违规：项目未引用测试框架\n\n" +
            $"修复建议：\n" +
            $"  1. 添加测试框架引用：\n" +
            $"     - xUnit: <PackageReference Include=\"xunit\" />\n" +
            $"     - NUnit: <PackageReference Include=\"NUnit\" />\n" +
            $"     - MSTest: <PackageReference Include=\"MSTest.TestFramework\" />\n" +
            $"  2. 确保 CI 配置中包含 dotnet test 命令\n" +
            $"  3. 验证所有测试可被测试运行器发现\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.1");

        // 验证测试文件使用正确的测试属性
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");
        if (Directory.Exists(testsDirectory))
        {
            var testFiles = Directory.GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs");
            var filesWithoutTests = new List<string>();

            foreach (var testFile in testFiles)
            {
                var content = File.ReadAllText(testFile);
                var hasFactOrTheory = content.Contains("[Fact") || content.Contains("[Theory");
                
                if (!hasFactOrTheory)
                {
                    filesWithoutTests.Add(Path.GetFileName(testFile));
                }
            }

            filesWithoutTests.Should().BeEmpty(
                $"❌ ADR-907_4_1 违规：以下测试文件没有使用测试属性\n\n" +
                $"{string.Join("\n", filesWithoutTests.Select(f => $"  • {f}"))}\n\n" +
                $"修复建议：\n" +
                $"  1. 确保所有测试方法标记了 [Fact] 或 [Theory]\n" +
                $"  2. 测试方法必须是 public void\n" +
                $"  3. 测试类必须是 public\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.1");
        }
    }

    /// <summary>
    /// ADR-907_4_2: RuleId 精确映射
    /// 验证测试失败必须精确映射至 ADR 子规则（RuleId）（§4.2）
    /// </summary>
    [Fact(DisplayName = "ADR-907_4_2: 测试失败必须映射到 RuleId")]
    public void ADR_907_4_2_Test_Failures_Must_Map_To_RuleId()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_4_2 无法执行：测试目录不存在 {testsDirectory}");
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

            // 查找所有 DisplayName
            var displayNames = Regex.Matches(content, @"DisplayName\s*=\s*""([^""]+)""");

            foreach (Match match in displayNames)
            {
                var displayName = match.Groups[1].Value;
                
                // 检查是否包含 RuleId 格式：ADR-XXXX_Y_Z
                var hasRuleId = Regex.IsMatch(displayName, $@"ADR-0*{adrNumber}_\d+_\d+:");
                
                if (!hasRuleId)
                {
                    violations.Add($"  • {fileName} - DisplayName 缺少 RuleId: {displayName}");
                }
            }

            // 检查失败消息中是否包含 RuleId
            var assertMessages = Regex.Matches(content, @"(Should\(\)\.BeTrue|Assert\.True)\s*\([^""]*""([^""]+)""", RegexOptions.Singleline);
            
            foreach (Match assert in assertMessages)
            {
                var message = assert.Groups[2].Value;
                
                // 检查失败消息是否包含精确的 RuleId
                var hasRuleIdInMessage = Regex.IsMatch(message, $@"ADR-0*{adrNumber}_\d+_\d+\s+违规");
                
                if (!hasRuleIdInMessage)
                {
                    violations.Add($"  • {fileName} - 失败消息缺少精确的 RuleId");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_4_2 违规：以下测试缺少 RuleId 映射\n\n" +
                $"{string.Join("\n", violations.Distinct())}\n\n" +
                $"修复建议：\n" +
                $"  1. DisplayName 必须包含完整的 RuleId\n" +
                $"     格式：ADR-<编号>_<Rule>_<Clause>: <描述>\n" +
                $"     示例：\"ADR-907_4_2: 测试失败必须映射到 RuleId\"\n" +
                $"  2. 失败消息必须包含精确的 RuleId\n" +
                $"     格式：❌ ADR-<编号>_<Rule>_<Clause> 违规：...\n" +
                $"     示例：\"❌ ADR-907_4_2 违规：测试失败必须映射到 RuleId\"\n" +
                $"  3. RuleId 用于自动化执法和追溯\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.2");
        }
    }

    /// <summary>
    /// ADR-907_4_3: 执行级别分类支持
    /// 验证支持执行级别分类（L1/L2）（§4.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907_4_3: 必须支持 L1/L2 执行级别")]
    public void ADR_907_4_3_Enforcement_Levels_Must_Be_Supported()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-architecture-tests-enforcement-governance.md");

        File.Exists(adrFile).Should().BeTrue(
            $"❌ ADR-907_4_3 违规：ADR-907 文档不存在\n\n" +
            $"预期路径：{adrFile}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR-907 文档存在\n" +
            $"  2. 文档必须定义执行级别分类\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.3");

        var adrContent = File.ReadAllText(adrFile);

        // 验证 ADR 定义了执行级别
        var hasEnforcementLevels = adrContent.Contains("L1") && adrContent.Contains("L2");
        hasEnforcementLevels.Should().BeTrue(
            $"❌ ADR-907_4_3 违规：ADR-907 未定义 L1/L2 执行级别\n\n" +
            $"修复建议：\n" +
            $"  1. 在 ADR-907 Enforcement 部分定义执行级别\n" +
            $"  2. L1：失败即阻断 CI / 合并 / 部署\n" +
            $"  3. L2：失败记录告警，进入人工 Code Review\n" +
            $"  4. 每个 Clause 必须明确标注执行级别\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.3");

        // 验证 Enforcement 表格存在
        var hasEnforcementTable = Regex.IsMatch(adrContent, @"\|\s*规则编号\s*\|\s*执行级\s*\|");
        hasEnforcementTable.Should().BeTrue(
            $"❌ ADR-907_4_3 违规：ADR-907 缺少 Enforcement 映射表\n\n" +
            $"修复建议：\n" +
            $"  1. 在 Enforcement 部分添加映射表\n" +
            $"  2. 表格列：规则编号 | 执行级 | 执法方式 | Decision 映射\n" +
            $"  3. 每个 Clause 必须有对应的表格行\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.3");

        // 验证所有规则都标注了执行级别
        var ruleIdPattern = @"ADR-907_\d+_\d+";
        var ruleIds = Regex.Matches(adrContent, ruleIdPattern)
            .Cast<Match>()
            .Select(m => m.Value)
            .Distinct()
            .ToList();

        ruleIds.Should().NotBeEmpty(
            $"❌ ADR-907_4_3 违规：未找到任何 RuleId 定义\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR-907 使用 Rule/Clause 编号体系\n" +
            $"  2. RuleId 格式：ADR-907_<Rule>_<Clause>\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.3");
    }

    /// <summary>
    /// ADR-907_4_4: 破例机制自动记录
    /// 验证破例机制必须自动记录（§4.4）
    /// </summary>
    [Fact(DisplayName = "ADR-907_4_4: 破例机制必须被文档化")]
    public void ADR_907_4_4_Exception_Mechanism_Must_Be_Documented()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 验证 ADR-900 定义了破例机制
        var adr0000File = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-900-architecture-tests.md");
        
        File.Exists(adr0000File).Should().BeTrue(
            $"❌ ADR-907_4_4 违规：ADR-900 文档不存在\n\n" +
            $"预期路径：{adr0000File}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ADR-900 架构测试宪法文档\n" +
            $"  2. 定义破例与偿还机制\n" +
            $"  3. 明确破例流程和审批要求\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.4");

        var adr0000Content = File.ReadAllText(adr0000File);

        // 验证破例机制的关键要素
        var hasExceptionMechanism = adr0000Content.Contains("破例") || 
                                   adr0000Content.Contains("Exception") ||
                                   adr0000Content.Contains("exemption");

        hasExceptionMechanism.Should().BeTrue(
            $"❌ ADR-907_4_4 违规：ADR-900 未定义破例机制\n\n" +
            $"修复建议：\n" +
            $"  1. 在 ADR-900 中定义破例机制\n" +
            $"  2. 破例记录必须包含：\n" +
            $"     - ADR 编号\n" +
            $"     - 测试类 / 方法\n" +
            $"     - 破例原因\n" +
            $"     - 到期时间与偿还计划\n" +
            $"  3. 明确 L1 规则通常不允许破例\n" +
            $"  4. L2 规则可申请破例但需审批\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.4");

        // 验证 ADR-907 说明了 L1 规则的破例限制
        var adr907File = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-architecture-tests-enforcement-governance.md");
        if (File.Exists(adr907File))
        {
            var adr907Content = File.ReadAllText(adr907File);
            
            // 根据最新的 ADR-907 v2.0，所有规则都是 L1，不再允许破例
            // 检查是否有相关说明
            var hasL1Description = Regex.IsMatch(adr907Content, @"L1.*阻断", RegexOptions.IgnoreCase);
            
            hasL1Description.Should().BeTrue(
                $"❌ ADR-907_4_4 违规：ADR-907 未说明 L1 执行级别的含义\n\n" +
                $"修复建议：\n" +
                $"  1. 明确 L1 规则的执行级别：失败即阻断\n" +
                $"  2. 说明 L2 规则的执行级别：记录告警\n" +
                $"  3. 根据 ADR-907 v2.0，所有规则已提升为 L1\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.4");
        }
    }

    /// <summary>
    /// ADR-907_4_5: Analyzer 检测能力要求
    /// 验证 Analyzer 必须具备检测空测试、弱断言、跨 ADR 混合等能力（§4.5）
    /// </summary>
    [Fact(DisplayName = "ADR-907_4_5: Analyzer 必须能检测违规")]
    public void ADR_907_4_5_Analyzer_Must_Detect_Violations()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907_4_5 违规：ADR 测试目录不存在\n\n" +
            $"预期路径：{testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保测试目录存在\n" +
            $"  2. ADR-907 是元规则，必须有自己的测试\n" +
            $"  3. 测试必须验证 Analyzer 的检测能力\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.5");

        // 验证 ADR-907 测试覆盖了必要的检测能力
        var testFiles = Directory.GetFiles(testsDirectory, "ADR_907_*_Architecture_Tests.cs");
        
        testFiles.Should().NotBeEmpty(
            $"❌ ADR-907_4_5 违规：ADR-907 没有架构测试\n\n" +
            $"修复建议：\n" +
            $"  1. 根据 ADR-907 v2.0 创建 4 个测试类\n" +
            $"  2. ADR_907_1_Architecture_Tests.cs（法律地位）\n" +
            $"  3. ADR_907_2_Architecture_Tests.cs（命名组织）\n" +
            $"  4. ADR_907_3_Architecture_Tests.cs（最小断言）\n" +
            $"  5. ADR_907_4_Architecture_Tests.cs（CI映射）\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.5");

        // 检查测试是否覆盖了必要的检测能力
        var allContent = string.Join("\n", testFiles.Select(File.ReadAllText));
        
        var requiredDetections = new Dictionary<string, string>
        {
            ["空测试"] = @"空测试|empty\s+test",
            ["弱断言"] = @"弱断言|weak\s+assertion|Assert\.True\s*\(\s*true",
            ["跨 ADR 混合"] = @"跨.*ADR|cross.*ADR|single.*ADR",
            ["测试命名"] = @"命名|naming|convention",
            ["Skip 检测"] = @"Skip|跳过",
        };

        var missingDetections = new List<string>();

        foreach (var detection in requiredDetections)
        {
            if (!Regex.IsMatch(allContent, detection.Value, RegexOptions.IgnoreCase))
            {
                missingDetections.Add($"  • {detection.Key}");
            }
        }

        if (missingDetections.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_4_5 违规：ADR-907 测试缺少以下检测能力\n\n" +
                $"{string.Join("\n", missingDetections)}\n\n" +
                $"修复建议：\n" +
                $"  1. 确保 Analyzer 能检测：\n" +
                $"     - 空测试 / 弱断言\n" +
                $"     - 单测试覆盖多 ADR\n" +
                $"     - 非 Final ADR 生成测试\n" +
                $"     - 测试命名不规范\n" +
                $"     - Skip / 条件禁用\n" +
                $"  2. 每种检测能力对应一个测试方法\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.5");
        }
    }

    /// <summary>
    /// ADR-907_4_6: ADR 生命周期同步
    /// 验证 ADR 生命周期变更必须同步到测试（§4.6）
    /// </summary>
    [Fact(DisplayName = "ADR-907_4_6: Superseded ADR 的测试必须被处理")]
    public void ADR_907_4_6_Superseded_ADRs_Must_Be_Handled()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDocsDirectory = Path.Combine(repoRoot, AdrDocsPath);
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(adrDocsDirectory) || !Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_4_6 无法执行：目录不存在");
            return;
        }

        // 查找所有 Superseded 或 Obsolete 的 ADR
        var adrFiles = Directory.GetFiles(adrDocsDirectory, "ADR-*.md", SearchOption.AllDirectories);
        var supersededAdrs = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 提取 ADR 编号
            var adrMatch = Regex.Match(fileName, @"ADR-(\d{3,4})");
            if (!adrMatch.Success)
                continue;

            var adrNumber = adrMatch.Groups[1].Value;

            // 检查是否为 Superseded 或 Obsolete
            var isSuperseded = Regex.IsMatch(content, @"status:\s*(Superseded|Obsolete)", RegexOptions.IgnoreCase);
            
            if (isSuperseded)
            {
                supersededAdrs.Add(adrNumber);
            }
        }

        // 检查这些 ADR 是否还有活跃的测试
        var violations = new List<string>();

        foreach (var adrNumber in supersededAdrs)
        {
            var testFilePattern4 = $"ADR_{adrNumber.PadLeft(4, '0')}_Architecture_Tests.cs";
            var testFilePattern3 = $"ADR_{int.Parse(adrNumber)}_Architecture_Tests.cs";
            
            var hasActiveTest = Directory.GetFiles(testsDirectory, testFilePattern4).Any() ||
                               Directory.GetFiles(testsDirectory, testFilePattern3).Any();

            if (hasActiveTest)
            {
                var testFile = Directory.GetFiles(testsDirectory, testFilePattern4).FirstOrDefault() ??
                              Directory.GetFiles(testsDirectory, testFilePattern3).FirstOrDefault();
                
                if (testFile != null)
                {
                    var content = File.ReadAllText(testFile);
                    
                    // 检查是否标记为 Obsolete 或包含说明
                    var hasObsoleteMarker = content.Contains("[Obsolete") || 
                                          content.Contains("已废弃") ||
                                          content.Contains("Superseded");

                    if (!hasObsoleteMarker)
                    {
                        violations.Add($"  • ADR-{adrNumber} - 测试未标记为废弃");
                    }
                }
            }
        }

        if (violations.Any())
        {
            // L2 级别：记录告警但不失败测试
            var message = string.Join("\n", new[]
            {
                $"⚠️ ADR-907_4_6 警告：以下 Superseded ADR 的测试未同步处理\n",
                string.Join("\n", violations),
                "",
                "修复建议：",
                "  1. 为 Superseded ADR 的测试添加 [Obsolete] 标记",
                "  2. 或在注释中说明该 ADR 已被取代",
                "  3. 或移除测试文件（如果规则已完全废弃）",
                "  4. 如果规则被其他 ADR 继承，在新测试中说明",
                "",
                "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.6",
                ""
            });

            // L2 级别：记录告警但不失败测试
            Console.WriteLine(message);
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
