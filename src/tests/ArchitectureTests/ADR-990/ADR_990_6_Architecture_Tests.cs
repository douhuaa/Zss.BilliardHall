using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_6: 路线图失效与阻断机制
/// 验证路线图过期视为治理失效的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_6_1: 路线图过期视为治理失效
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_6_Architecture_Tests
{
    [Fact(DisplayName = "ADR-990_6_1: 路线图过期视为治理失效")]
    public void ADR_990_6_1_Stale_Roadmap_Is_Governance_Failure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        content.Should().Contain("路线图过期",
            $"❌ ADR-990_6_1 违规：ADR-990 必须定义路线图过期机制\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §6.1");

        content.Should().Contain("治理失效",
            $"❌ ADR-990_6_1 违规：ADR-990 必须将路线图过期视为治理失效");
    }
}
