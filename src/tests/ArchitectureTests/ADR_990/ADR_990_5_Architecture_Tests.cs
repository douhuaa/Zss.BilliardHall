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
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_5_1",
            filePath: adr990Path,
            missingContent: "公开可访问",
            remediationSteps: new[]
            {
                "在 ADR-990 中要求路线图公开可访问",
                "确保所有团队成员都能查看路线图"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("公开可访问", missingMessage);
    }

}
