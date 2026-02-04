using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_980;

/// <summary>
/// ADR-980_3: 变更传播清单
/// 验证修改 ADR 时必须遵循变更传播清单的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-980_3_1: 修改 ADR 时必须遵循变更传播清单
/// - ADR-980_3_2: 标准清单验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md
/// - version: 2.0
/// </summary>
public sealed class ADR_980_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-980_3_1: 修改 ADR 时必须遵循变更传播清单")]
    public void ADR_980_3_1_Changes_Must_Follow_Propagation_Checklist()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        var content = File.ReadAllText(adr980Path);

        content.Should().Contain("变更传播清单",
            $"❌ ADR-980_3_1 违规：ADR-980 必须定义变更传播清单\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §3.1");
    }

    [Fact(DisplayName = "ADR-980_3_2: 必须定义标准清单")]
    public void ADR_980_3_2_Standard_Checklist_Must_Be_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr980Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md");

        var content = File.ReadAllText(adr980Path);

        content.Should().Contain("标记版本变更",
            $"❌ ADR-980_3_2 违规：标准清单必须包含'标记版本变更'步骤\n\n" +
            $"参考：docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md §3.2");
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
