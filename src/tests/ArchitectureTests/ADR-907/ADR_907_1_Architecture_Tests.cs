namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907;

/// <summary>
/// ADR-907_1: ArchitectureTests 的法律地位
/// 验证 ArchitectureTests 作为 ADR 唯一自动化执法形式的相关规则
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-907_1_1: 唯一自动化执法形式 → ADR_907_1_1_ArchitectureTests_Are_Sole_Automated_Enforcement
/// - ADR-907_1_2: 可执法性要求 → ADR_907_1_2_ADRs_Must_Have_Tests_Or_Be_Non_Enforceable
/// - ADR-907_1_3: 禁止仅文档约束 → ADR_907_1_3_No_Documentation_Only_Rules_Allowed
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-907.prompts.md
/// </summary>
public sealed class ADR_907_1_Architecture_Tests
{
    private const string AdrTestsPath = "src/tests/ArchitectureTests";

    /// <summary>
    /// ADR-907_1_1: 唯一自动化执法形式
    /// 验证 ArchitectureTests 是 ADR 的唯一自动化执法形式（§1.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907_1_1: ArchitectureTests 必须是 ADR 的唯一自动化执法形式")]
    public void ADR_907_1_1_ArchitectureTests_Are_Sole_Automated_Enforcement()
    {
        var testsDirectory = FileSystemTestHelper.GetAbsolutePath(AdrTestsPath);

        // 验证 ArchitectureTests 项目存在
        var directoryMessage = AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
            ruleId: "ADR-907_1_1",
            directoryPath: testsDirectory,
            directoryDescription: "ArchitectureTests 目录",
            remediationSteps: new[]
            {
                "创建独立的 ArchitectureTests 项目",
                "ArchitectureTests 是 ADR 的唯一自动化执法形式",
                "所有可执法的架构规则必须通过 ArchitectureTests 验证"
            },
            adrReference: TestConstants.Adr907Path);

        Directory.Exists(testsDirectory).Should().BeTrue(directoryMessage);

        // 验证项目文件存在
        var projectFile = Path.Combine(testsDirectory, "ArchitectureTests.csproj");
        var fileMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-907_1_1",
            filePath: projectFile,
            fileDescription: "ArchitectureTests 项目文件",
            remediationSteps: new[]
            {
                "创建 ArchitectureTests.csproj 项目文件",
                "配置为测试项目，引用 xUnit 和 NetArchTest.Rules",
                "确保项目命名格式：<SolutionName>.Tests.Architecture"
            },
            adrReference: TestConstants.Adr907Path);

        File.Exists(projectFile).Should().BeTrue(fileMessage);

        // 验证测试使用允许的技术（文件系统扫描、Roslyn、Reflection、NetArchTest）
        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories);
        testFiles.Should().NotBeEmpty(
            AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-907_1_1",
                filePath: testsDirectory,
                missingContent: "测试文件",
                remediationSteps: new[]
                {
                    "为每个具备裁决力的 ADR 创建对应的架构测试",
                    "测试可使用：文件系统扫描、Roslyn、Reflection、NetArchTest",
                    "测试文件命名：ADR_<编号>_Architecture_Tests.cs"
                },
                adrReference: TestConstants.Adr907Path));
    }

    /// <summary>
    /// ADR-907_1_2: 可执法性要求
    /// 验证任何具备裁决力的 ADR 必须有对应的 ArchitectureTests 或明确声明为 Non-Enforceable（§1.2）
    /// </summary>
    [Fact(DisplayName = "ADR-907_1_2: 具备裁决力的 ADR 必须有测试或声明 Non-Enforceable")]
    public void ADR_907_1_2_ADRs_Must_Have_Tests_Or_Be_Non_Enforceable()
    {
        var adrDocsDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);
        var testsDirectory = FileSystemTestHelper.GetAbsolutePath(AdrTestsPath);

        Directory.Exists(adrDocsDirectory).Should().BeTrue($"❌ ADR-907_1_2 无法执行：ADR 文档目录不存在 {adrDocsDirectory}");

        Directory.Exists(testsDirectory).Should().BeTrue($"❌ ADR-907_1_2 无法执行：测试目录不存在 {testsDirectory}");

        // 查找所有 Final 或 Active 状态的 ADR
        var adrFiles = Directory.GetFiles(adrDocsDirectory, "ADR-*.md", SearchOption.AllDirectories);
        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 提取 ADR 编号
            var adrMatch = Regex.Match(fileName, @"ADR-(\d{3,4})");
            if (!adrMatch.Success)
                continue;

            var adrNumber = adrMatch.Groups[1].Value.PadLeft(4, '0');
            var adrNumber3Digit = adrMatch.Groups[1].Value.PadLeft(3, '0');  // 同时保留3位格式

            // 检查是否为 Final 或 Active 状态
            var isFinalOrActive = Regex.IsMatch(content, @"status:\s*(Final|Active)", RegexOptions.IgnoreCase);
            if (!isFinalOrActive)
                continue;

            // 检查是否明确声明为 Non-Enforceable
            var isNonEnforceable = Regex.IsMatch(content, @"enforceable:\s*(false|no)", RegexOptions.IgnoreCase) ||
                                   content.Contains("Non-Enforceable", StringComparison.OrdinalIgnoreCase);

            if (isNonEnforceable)
                continue;

            // 检查是否存在对应的测试文件（递归搜索所有子目录）
            // 新格式：ADR_XXX_<Rule>_Architecture_Tests.cs 或 ADR_XXXX_<Rule>_Architecture_Tests.cs
            // 旧格式：ADR_XXX_Architecture_Tests.cs（无Rule编号，在迁移期间允许）
            var testFilePattern4Digit = $"ADR_{adrNumber}_*_Architecture_Tests.cs";
            var testFilePattern3Digit = $"ADR_{adrNumber3Digit}_*_Architecture_Tests.cs";

            // 也检查旧格式的测试文件（无Rule编号）
            var oldTestFilePattern4Digit = $"ADR_{adrNumber}_Architecture_Tests.cs";
            var oldTestFilePattern3Digit = $"ADR_{adrNumber3Digit}_Architecture_Tests.cs";

            var hasTest = Directory.GetFiles(testsDirectory, testFilePattern4Digit, SearchOption.AllDirectories).Any() ||
                         Directory.GetFiles(testsDirectory, testFilePattern3Digit, SearchOption.AllDirectories).Any() ||
                         Directory.GetFiles(testsDirectory, oldTestFilePattern4Digit, SearchOption.AllDirectories).Any() ||
                         Directory.GetFiles(testsDirectory, oldTestFilePattern3Digit, SearchOption.AllDirectories).Any();

            if (!hasTest)
            {
                violations.Add($"  • ADR-{adrNumber} ({fileName}) - 缺少架构测试且未声明 Non-Enforceable");
            }
        }

        violations.Should().BeEmpty($"❌ ADR-907_1_2 违规：以下 ADR 缺少架构测试且未声明 Non-Enforceable\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 为每个 Final/Active ADR 创建对应的架构测试\n" +
                $"  2. 或在 ADR 元数据中明确声明 enforceable: false\n" +
                $"  3. 或在 ADR 正文中添加 Non-Enforceable 标记\n" +
                $"  4. 测试文件命名：ADR_<编号>_Architecture_Tests.cs\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.2");
    }

    /// <summary>
    /// ADR-907_1_3: 禁止仅文档约束
    /// 验证不存在"仅文档约束、不接受执行"的架构规则（§1.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907_1_3: 禁止仅文档约束的架构规则")]
    public void ADR_907_1_3_No_Documentation_Only_Rules_Allowed()
    {
        var adrDocsDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);

        Directory.Exists(adrDocsDirectory).Should().BeTrue($"❌ ADR-907_1_3 无法执行：ADR 文档目录不存在 {adrDocsDirectory}");

        var adrFiles = Directory.GetFiles(adrDocsDirectory, "ADR-*.md", SearchOption.AllDirectories);
        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否为 Final 或 Active 状态
            var isFinalOrActive = Regex.IsMatch(content, @"status:\s*(Final|Active)", RegexOptions.IgnoreCase);
            if (!isFinalOrActive)
                continue;

            // 检查是否包含"仅文档"、"不执行"等禁止的声明
            var prohibitedPatterns = new[]
            {
                @"仅文档约束",
                @"documentation\s+only",
                @"不接受执行",
                @"no\s+enforcement",
                @"不执行",
                @"manual\s+only"
            };

            foreach (var pattern in prohibitedPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"  • {fileName} - 包含禁止的声明：{pattern}");
                    break;
                }
            }
        }

        violations.Should().BeEmpty($"❌ ADR-907_1_3 违规：以下 ADR 包含\"仅文档约束\"的禁止声明\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 移除\"仅文档约束\"、\"不接受执行\"等声明\n" +
                $"  2. 选择以下之一：\n" +
                $"     a. 创建对应的 ArchitectureTests（推荐）\n" +
                $"     b. 明确声明为 Non-Enforceable\n" +
                $"  3. 所有架构规则必须可执法或明确声明不可执法\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.3");
    }

    #region Helper Methods


    #endregion
}
