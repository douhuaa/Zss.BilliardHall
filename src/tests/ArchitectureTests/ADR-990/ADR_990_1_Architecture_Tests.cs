namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_1: 路线图结构
/// 验证文档演进路线图的结构和组织规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_1_1: 路线图必须按标准结构组织
/// - ADR-990_1_2: 文档位置验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_1_Architecture_Tests
{
    /// <summary>
    /// ADR-990_1_1: 路线图结构验证
    /// 验证 ADR-990 定义了路线图结构（§ADR-990_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-990_1_1: 路线图必须按标准结构组织")]
    public void ADR_990_1_1_Roadmap_Structure_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        File.Exists(adr990Path).Should().BeTrue(
            $"❌ ADR-990_1_1 违规：ADR-990 文档不存在\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §1.1");

        var content = File.ReadAllText(adr990Path);

        // 验证定义了路线图结构
        content.Should().Contain("标准结构",
            $"❌ ADR-990_1_1 违规：ADR-990 必须定义路线图标准结构\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §1.1");
    }

    /// <summary>
    /// ADR-990_1_2: 文档位置验证
    /// 验证定义了路线图文档位置（§ADR-990_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-990_1_2: 必须定义路线图文档位置")]
    public void ADR_990_1_2_Document_Location_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = File.ReadAllText(adr990Path);

        // 验证定义了文档位置
        content.Should().Contain("docs/ROADMAP.md",
            $"❌ ADR-990_1_2 违规：ADR-990 必须定义路线图文档位置为 docs/ROADMAP.md\n\n" +
            $"参考：docs/adr/governance/ADR-990-documentation-evolution-roadmap.md §1.2");
    }

}
