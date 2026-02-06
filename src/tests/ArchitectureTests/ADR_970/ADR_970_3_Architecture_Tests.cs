namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_3: 日志与 ADR 关联机制
/// 验证测试失败日志必须链接到 ADR 的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_3_1: 测试失败日志必须自动链接到 ADR
/// - ADR-970_3_2: 关联规则验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-970_3_1: 测试失败日志必须自动链接到 ADR")]
    public void ADR_970_3_1_Test_Failure_Logs_Must_Link_To_ADR()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        content.Should().Contain("自动链接到",
            $"❌ ADR-970_3_1 违规：ADR-970 必须定义自动链接到 ADR 的机制\n\n" +
            $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §3.1");
    }

    [Fact(DisplayName = "ADR-970_3_2: 必须定义关联规则")]
    public void ADR_970_3_2_Association_Rules_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        content.Should().Contain("关联规则",
            $"❌ ADR-970_3_2 违规：ADR-970 必须定义关联规则\n\n" +
            $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §3.2");
    }

}
