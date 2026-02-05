using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_4: 质量阈值与阻断策略
/// 验证最低质量阈值和阻断策略的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_4_1: 最低质量阈值验证
/// - ADR-975_4_2: 硬失败项验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-975_4_1: 必须定义最低质量阈值")]
    public void ADR_975_4_1_Minimum_Quality_Threshold_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr975Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = File.ReadAllText(adr975Path);

        content.Should().Contain("≥ 85%",
            $"❌ ADR-975_4_1 违规：ADR-975 必须定义最低质量阈值为 85%\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §4.1");
    }

    [Fact(DisplayName = "ADR-975_4_2: 必须定义硬失败项")]
    public void ADR_975_4_2_Hard_Failure_Items_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr975Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = File.ReadAllText(adr975Path);

        content.Should().Contain("硬失败项",
            $"❌ ADR-975_4_2 违规：ADR-975 必须定义硬失败项\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §4.2");

        // 验证包含关键硬失败项
        content.Should().Contain("链接失效",
            $"❌ ADR-975_4_2 违规：硬失败项必须包含'链接失效'");
    }
}
