using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_3: 与 ADR/RFC 的关联
/// 验证路线图项目必须与 ADR/RFC 关联的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_3_1: 路线图项目必须与 ADR/RFC 关联
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-990_3_1: 路线图项目必须与 ADR/RFC 关联")]
    public void ADR_990_3_1_Roadmap_Items_Must_Link_To_ADR_Or_RFC()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        content.Should().Contain("与 ADR/RFC 关联",
            $"❌ ADR-990_3_1 违规：ADR-990 必须要求路线图项目与 ADR/RFC 关联\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §3.1");
    }
}
