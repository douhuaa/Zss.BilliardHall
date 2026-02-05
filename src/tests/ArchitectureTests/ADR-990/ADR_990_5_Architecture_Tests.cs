using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_5: 透明度与可访问性
/// 验证路线图必须公开可访问的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_5_1: 路线图必须公开可访问
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_5_Architecture_Tests
{
    [Fact(DisplayName = "ADR-990_5_1: 路线图必须公开可访问")]
    public void ADR_990_5_1_Roadmap_Must_Be_Publicly_Accessible()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        content.Should().Contain("公开可访问",
            $"❌ ADR-990_5_1 违规：ADR-990 必须要求路线图公开可访问\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §5.1");
    }
}
