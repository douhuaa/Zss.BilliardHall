namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_990;

/// <summary>
/// ADR-990_6: 路线图失效与阻断机制
/// 验证路线图过期视为治理失效的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-990_6_1: 路线图过期视为治理失效
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-990-documentation-evolution-roadmap.md
/// - version: 2.0
/// </summary>
public sealed class ADR_990_6_Architecture_Tests
{
    [Fact(DisplayName = "ADR-990_6_1: 路线图过期视为治理失效")]
    public void ADR_990_6_1_Stale_Roadmap_Is_Governance_Failure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        var missingMessage1 = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_6_1",
            filePath: adr990Path,
            missingContent: "路线图过期",
            remediationSteps: new[]
            {
                "在 ADR-990 中定义路线图过期机制",
                "说明路线图过期的判定标准"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("路线图过期", missingMessage1);

        var missingMessage2 = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_6_1",
            filePath: adr990Path,
            missingContent: "治理失效",
            remediationSteps: new[]
            {
                "在 ADR-990 中明确将路线图过期视为治理失效",
                "定义治理失效后的处理流程"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("治理失效", missingMessage2);
    }

}
