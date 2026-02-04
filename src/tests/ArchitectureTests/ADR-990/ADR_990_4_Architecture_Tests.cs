using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_4: 状态追踪
/// 验证必须实时追踪项目状态的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_4_1: 必须实时追踪项目状态
/// - ADR-990_4_2: 状态定义验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-990_4_1: 必须实时追踪项目状态")]
    public void ADR_990_4_1_Project_Status_Must_Be_Tracked()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        content.Should().Contain("实时追踪项目状态",
            $"❌ ADR-990_4_1 违规：ADR-990 必须要求实时追踪项目状态\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §4.1");
    }

    [Fact(DisplayName = "ADR-990_4_2: 必须定义状态")]
    public void ADR_990_4_2_Status_Definitions_Must_Be_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        var requiredStatuses = new[] { "完成", "进行中", "延迟", "规划中" };
        foreach (var status in requiredStatuses)
        {
            content.Should().Contain(status,
                $"❌ ADR-990_4_2 违规：状态定义必须包含 '{status}'\n\n" +
                $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §4.2");
        }
    }

    private static string? FindRepositoryRoot()
    {
        var directory = Directory.GetCurrentDirectory();
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory, ".git")))
            {
                return directory;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }
        return null;
    }
}
