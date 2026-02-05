using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_2: 自动化检测机制
/// 验证质量检测必须自动化执行的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_2_1: 质量检测必须自动化执行
/// - ADR-975_2_2: 链接有效性检测
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-975_2_1: 质量检测必须自动化执行")]
    public void ADR_975_2_1_Quality_Checks_Must_Be_Automated()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr975Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = File.ReadAllText(adr975Path);

        content.Should().Contain("自动化执行",
            $"❌ ADR-975_2_1 违规：ADR-975 必须要求质量检测自动化执行\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §2.1");
    }

    [Fact(DisplayName = "ADR-975_2_2: 必须包含链接有效性检测")]
    public void ADR_975_2_2_Link_Validity_Check_Must_Be_Included()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr975Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = File.ReadAllText(adr975Path);

        content.Should().Contain("链接有效性检测",
            $"❌ ADR-975_2_2 违规：ADR-975 必须包含链接有效性检测\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §2.2");
    }

}
