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
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_2_1",
            filePath: adr990Path,
            missingContent: "每季度更新",
            remediationSteps: new[]
            {
                "在 ADR-990 中添加每季度更新路线图的要求",
                "明确更新频率和时间节点"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("每季度更新", missingMessage);
    }

}
