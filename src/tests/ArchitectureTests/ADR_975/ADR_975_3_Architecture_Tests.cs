namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_3: 定期报告生成
/// 验证必须每月生成文档质量报告的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_3_1: 必须每月生成文档质量报告
/// - ADR-975_3_2: 报告位置验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-975_3_1: 必须每月生成文档质量报告")]
    public void ADR_975_3_1_Monthly_Quality_Report_Must_Be_Generated()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr975Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = File.ReadAllText(adr975Path);

        content.Should().Contain("每月生成",
            $"❌ ADR-975_3_1 违规：ADR-975 必须要求每月生成文档质量报告\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §3.1");
    }

    [Fact(DisplayName = "ADR-975_3_2: 必须定义报告位置")]
    public void ADR_975_3_2_Report_Location_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr975Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = File.ReadAllText(adr975Path);

        content.Should().Contain("docs/reports/quality",
            $"❌ ADR-975_3_2 违规：ADR-975 必须定义报告位置\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §3.2");
    }

}
