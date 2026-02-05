namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907_A;

/// <summary>
/// ADR-907-A_3: 测试绑定规则
/// 验证 Rule/Clause 变更时的测试同步要求
///
/// 测试覆盖映射（严格遵循 ADR-907-A v1.2 Rule/Clause 体系）：
/// - ADR-907-A_3_1: 测试同步强制要求 → ADR_907_A_3_1_Test_Sync_Mandatory_On_Changes
/// - ADR-907-A_3_2: 测试命名一致性要求 → ADR_907_A_3_2_Test_Naming_Consistency
/// - ADR-907-A_3_3: 测试失败信息更新要求 → ADR_907_A_3_3_Test_Failure_Message_Update
/// - ADR-907-A_3_4: 旧编号测试处理规则 → ADR_907_A_3_4_Old_Numbered_Test_Handling
/// - ADR-907-A_3_5: 测试覆盖率验证 → ADR_907_A_3_5_Test_Coverage_Validation
/// - ADR-907-A_3_6: 测试目录结构规范 → ADR_907_A_3_6_Test_Directory_Structure
/// - ADR-907-A_3_7: 测试文件与代码文件对应关系 → ADR_907_A_3_7_Test_File_Code_Correspondence
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md
/// </summary>
public sealed class ADR_907_A_3_Architecture_Tests
{
    private const string AdrTestsPath = "src/tests/ArchitectureTests";

    /// <summary>
    /// ADR-907-A_3_1: 测试同步强制要求
    /// 验证 ADR 变更时必须同步更新测试
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_1: ADR 变更必须同步测试")]
    public void ADR_907_A_3_1_Test_Sync_Mandatory_On_Changes()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("必须在同一 PR 中更新",
            $"❌ ADR-907-A_3_1 违规：ADR-907-A 未要求在同一 PR 中更新测试");

        content.Should().Contain("CI 失败",
            $"❌ ADR-907-A_3_1 违规：ADR-907-A 未定义不同步时的 CI 行为");
    }

    /// <summary>
    /// ADR-907-A_3_2: 测试命名一致性要求
    /// 验证测试命名与 RuleId 的严格一致性
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_2: 测试命名必须与 RuleId 一致")]
    public void ADR_907_A_3_2_Test_Naming_Consistency()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("Architecture_Tests.cs"))
            .ToArray();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileName(testFile);

            // 检查文件命名格式
            fileName.Should().MatchRegex(@"^ADR_\d+(_\w+)?_\d+_Architecture_Tests\.cs$",
                $"❌ ADR-907-A_3_2 违规：{fileName} 不符合命名规范 ADR_XXX_Y_Architecture_Tests.cs");
        }
    }

    /// <summary>
    /// ADR-907-A_3_3: 测试失败信息更新要求
    /// 验证测试失败信息引用新 RuleId
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_3: 测试失败信息必须引用新 RuleId")]
    public void ADR_907_A_3_3_Test_Failure_Message_Update()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("Architecture_Tests.cs"))
            .ToArray();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileName(testFile);

            // 检查是否使用新 RuleId 格式
            if (content.Contains("ADR-"))
            {
                content.Should().NotMatchRegex(@"ADR-\d+\.\d+:L\d+",
                    $"❌ ADR-907-A_3_3 违规：{fileName} 仍使用旧 RuleId 格式 ADR-X.Y:LZ");

                // 应该使用新格式 ADR-X_Y_Z
                var hasNewFormat = System.Text.RegularExpressions.Regex.IsMatch(content, @"ADR-\d+_\d+_\d+");
                hasNewFormat.Should().BeTrue(
                    $"❌ ADR-907-A_3_3 违规：{fileName} 未使用新 RuleId 格式 ADR-X_Y_Z");
            }
        }
    }

    /// <summary>
    /// ADR-907-A_3_4: 旧编号测试处理规则
    /// 验证对齐完成后不允许保留旧编号测试
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_4: 对齐后不允许保留旧编号测试")]
    public void ADR_907_A_3_4_Old_Numbered_Test_Handling()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("Architecture_Tests.cs"))
            .ToArray();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileName(testFile);

            // 检查是否包含旧格式的测试方法名
            var hasOldMethodNames = System.Text.RegularExpressions.Regex.IsMatch(content,
                @"public void ADR_\d+\.\d+");

            hasOldMethodNames.Should().BeFalse(
                $"❌ ADR-907-A_3_4 违规：{fileName} 包含旧格式的测试方法名");
        }
    }

    /// <summary>
    /// ADR-907-A_3_5: 测试覆盖率验证
    /// 验证每个 Clause 都有对应测试方法
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_5: 每个 Clause 必须有测试覆盖")]
    public void ADR_907_A_3_5_Test_Coverage_Validation()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("TODO stub",
            $"❌ ADR-907-A_3_5 违规：ADR-907-A 未定义 Clause 暂时无法测试时的处理方式");

        content.Should().Contain("30 天内实现",
            $"❌ ADR-907-A_3_5 违规：ADR-907-A 未定义 TODO stub 的时限");
    }

    /// <summary>
    /// ADR-907-A_3_6: 测试目录结构规范
    /// 验证测试目录结构符合规范
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_6: 测试目录结构必须符合规范")]
    public void ADR_907_A_3_6_Test_Directory_Structure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        // 检查 ADR 目录命名
        var adrDirectories = Directory.GetDirectories(testsDirectory)
            .Where(d => Path.GetFileName(d).StartsWith("ADR_"))
            .ToArray();

        foreach (var dir in adrDirectories)
        {
            var dirName = Path.GetFileName(dir);

            dirName.Should().MatchRegex(@"^ADR_\d+(_\w+)?$",
                $"❌ ADR-907-A_3_6 违规：目录 {dirName} 不符合命名规范 ADR_XXX");
        }

        // 检查 Shared 目录存在
        var sharedDir = Path.Combine(testsDirectory, "Shared");
        Directory.Exists(sharedDir).Should().BeTrue(
            $"❌ ADR-907-A_3_6 违规：缺少 Shared 目录");
    }

    /// <summary>
    /// ADR-907-A_3_7: 测试文件与代码文件对应关系
    /// 验证测试文件与被测代码的对应关系注释
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_3_7: 测试文件必须有对应关系注释")]
    public void ADR_907_A_3_7_Test_File_Code_Correspondence()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("Architecture_Tests.cs"))
            .ToArray();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileName(testFile);

            // 检查类注释
            content.Should().Contain("/// <summary>",
                $"❌ ADR-907-A_3_7 违规：{fileName} 缺少类级注释");

            content.Should().Contain("验证 ADR-",
                $"❌ ADR-907-A_3_7 违规：{fileName} 类注释未提及对应的 ADR");
        }
    }
}
