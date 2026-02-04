using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_2: 季度更新机制
/// 验证必须每季度更新路线图的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_2_1: 必须每季度更新路线图
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-990_2_1: 必须每季度更新路线图")]
    public void ADR_990_2_1_Roadmap_Must_Be_Updated_Quarterly()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        content.Should().Contain("每季度更新",
            $"❌ ADR-990_2_1 违规：ADR-990 必须要求每季度更新路线图\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §2.1");
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
