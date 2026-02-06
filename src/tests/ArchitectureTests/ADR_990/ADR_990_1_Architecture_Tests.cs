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

        var fileMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-990_1_1",
            filePath: adr990Path,
            fileDescription: "ADR-990 文档演进路线图",
            remediationSteps: new[]
            {
                "创建 ADR-990 文档",
                "定义路线图结构和组织规范"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        File.Exists(adr990Path).Should().BeTrue(fileMessage);

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        // 验证定义了路线图结构
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_1_1",
            filePath: adr990Path,
            missingContent: "标准结构",
            remediationSteps: new[]
            {
                "在 ADR-990 中定义路线图标准结构",
                "说明路线图应该如何组织和呈现"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("标准结构", missingMessage);
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

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        // 验证定义了文档位置
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_1_2",
            filePath: adr990Path,
            missingContent: "docs/ROADMAP.md",
            remediationSteps: new[]
            {
                "在 ADR-990 中定义路线图文档位置为 docs/ROADMAP.md",
                "明确路线图文档的存放位置"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("docs/ROADMAP.md", missingMessage);
    }

}
