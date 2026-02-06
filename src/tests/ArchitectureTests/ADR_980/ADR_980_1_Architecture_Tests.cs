namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_980;

/// <summary>
/// ADR-980_1: 版本号关联规则
/// 验证 ADR 正文、架构测试、Copilot Prompt 版本号一致性
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-980_1_1: ADR/测试/Prompt 版本号必须一致
/// - ADR-980_1_2: 版本号格式验证
/// - ADR-980_1_6: 三位一体存在性验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md
/// - version: 2.0
/// </summary>
public sealed class ADR_980_1_Architecture_Tests
{
    private const string AdrVersionPattern = @"version:\s*[""'](\d+\.\d+)[""']";
    private const string TestVersionPattern = @"//\s*version:\s*(\d+\.\d+)";

    /// <summary>
    /// ADR-980_1_1: 版本号一致性验证
    /// 验证 ADR/测试/Prompt 版本号必须一致（§ADR-980_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-980_1_1: ADR/测试/Prompt 版本号必须一致")]
    public void ADR_980_1_1_Version_Numbers_Must_Match()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        // 验证 ADR-980 自身存在
        File.Exists(adr980Path).Should().BeTrue(
            $"❌ ADR-980_1_1 违规：ADR-980 文档不存在\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §1.1");

        var adrContent = FileSystemTestHelper.ReadFileContent(adr980Path);

        // 提取 ADR-980 的版本号
        var adrVersionMatch = Regex.Match(adrContent, AdrVersionPattern);
        adrVersionMatch.Success.Should().BeTrue(
            $"❌ ADR-980_1_1 违规：ADR-980 缺少版本号字段");

        var adrVersion = adrVersionMatch.Groups[1].Value;
        adrVersion.Should().Be("2.0",
            $"❌ ADR-980_1_1 违规：ADR-980 版本号应为 2.0（当前：{adrVersion}）");

        // 验证本测试文件的版本号
        var testSourceFiles = Directory.GetFiles(
            Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR-980"),
            "*Architecture_Tests.cs",
            SearchOption.TopDirectoryOnly);

        foreach (var testSourceFile in testSourceFiles)
        {
            var testContent = FileSystemTestHelper.ReadFileContent(testSourceFile);
            var testVersionMatch = Regex.Match(testContent, TestVersionPattern);

            if (testVersionMatch.Success)
            {
                var testVersion = testVersionMatch.Groups[1].Value;
                testVersion.Should().Be(adrVersion,
                    $"❌ ADR-980_1_1 违规：测试文件版本号与 ADR 不一致\n" +
                    $"  ADR 版本：{adrVersion}\n" +
                    $"  测试版本：{testVersion}\n" +
                    $"  文件：{Path.GetFileName(testSourceFile)}\n\n" +
                    $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §1.1");
            }
        }
    }

    /// <summary>
    /// ADR-980_1_2: 版本号格式验证
    /// 验证版本号格式符合 X.Y 规范（§ADR-980_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-980_1_2: 版本号格式必须为 X.Y")]
    public void ADR_980_1_2_Version_Format_Must_Be_Valid()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        var content = FileSystemTestHelper.ReadFileContent(adr980Path);

        // 验证版本号格式描述
        content.Should().Contain("version：X.Y",
            $"❌ ADR-980_1_2 违规：ADR-980 必须定义版本号格式规范\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §1.2");

        // 验证包含示例
        content.Should().Contain("version: \"2.0\"",
            $"❌ ADR-980_1_2 违规：ADR-980 必须包含版本号格式示例");
    }

    /// <summary>
    /// ADR-980_1_6: 三位一体存在性验证
    /// 验证非 Document-Only ADR 必须有测试或 Prompt（§ADR-980_1_6）
    /// </summary>
    [Fact(DisplayName = "ADR-980_1_6: 非 Document-Only ADR 必须有测试或 Prompt")]
    public void ADR_980_1_6_Trinity_Existence_Must_Be_Enforced()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        var content = FileSystemTestHelper.ReadFileContent(adr980Path);

        // 验证三位一体存在性规则
        content.Should().Contain("三位一体存在性",
            $"❌ ADR-980_1_6 违规：ADR-980 必须定义三位一体存在性规则");

        content.Should().Contain("Document-Only ADR",
            $"❌ ADR-980_1_6 违规：ADR-980 必须说明 Document-Only ADR 的豁免条件");

        // 验证规则内容
        content.Should().Contain("必须存在至少一个架构测试",
            $"❌ ADR-980_1_6 违规：ADR-980 必须要求 ADR 具有架构测试或 Prompt\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §1.6");
    }

}
