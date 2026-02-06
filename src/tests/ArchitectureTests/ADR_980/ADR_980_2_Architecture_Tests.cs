namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_980;

/// <summary>
/// ADR-980_2: 同步检测自动化
/// 验证 CI 必须包含版本同步检测的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-980_2_1: CI 必须包含版本同步检测步骤
/// - ADR-980_2_4: 检测工具验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md
/// - version: 2.0
/// </summary>
public sealed class ADR_980_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-980_2_1: CI 必须包含版本同步检测步骤")]
    public void ADR_980_2_1_CI_Must_Include_Version_Sync_Detection()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        var content = FileSystemTestHelper.ReadFileContent(adr980Path);

        content.Should().Contain("CI 必须包含版本同步检测",
            $"❌ ADR-980_2_1 违规：ADR-980 必须要求 CI 包含版本同步检测步骤\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §2.1");
    }

    [Fact(DisplayName = "ADR-980_2_4: 必须定义检测工具")]
    public void ADR_980_2_4_Detection_Tools_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        var content = FileSystemTestHelper.ReadFileContent(adr980Path);

        content.Should().Contain("validate-adr-version-sync.sh",
            $"❌ ADR-980_2_4 违规：ADR-980 必须定义检测工具脚本\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §2.4");

        content.Should().Contain("adr-version-sync.yml",
            $"❌ ADR-980_2_4 违规：ADR-980 必须定义 CI Workflow");
    }

}
