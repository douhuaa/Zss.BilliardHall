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
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-990_4_1",
            filePath: adr990Path,
            missingContent: "实时追踪项目状态",
            remediationSteps: new[]
            {
                "在 ADR-990 中要求实时追踪项目状态",
                "定义状态追踪的方式和频率"
            },
            adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        content.Should().Contain("实时追踪项目状态", missingMessage);
    }

    [Fact(DisplayName = "ADR-990_4_2: 必须定义状态")]
    public void ADR_990_4_2_Status_Definitions_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr990Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

        var content = FileSystemTestHelper.ReadFileContent(adr990Path);

        var requiredStatuses = new[] { "完成", "进行中", "延迟", "规划中" };
        foreach (var status in requiredStatuses)
        {
            var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-990_4_2",
                filePath: adr990Path,
                missingContent: status,
                remediationSteps: new[]
                {
                    $"在 ADR-990 中添加状态定义：{status}",
                    "确保包含所有必需的项目状态类型"
                },
                adrReference: "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md");

            content.Should().Contain(status, missingMessage);
        }
    }

}
