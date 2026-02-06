namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_4: 自动化报告生成
/// 验证 CI 必须自动生成结构化日志的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_4_1: CI 必须自动生成结构化日志
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-970_4_1: CI 必须自动生成结构化日志")]
    public void ADR_970_4_1_CI_Must_Generate_Structured_Logs()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        var ciLogGenerationMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-970_4_1",
            filePath: adr970Path,
            missingContent: "CI 必须自动生成结构化日志",
            remediationSteps: new[]
            {
                "在 ADR-970 中要求 CI 自动生成结构化日志",
                "定义 CI 集成的具体要求"
            },
            adrReference: "docs/adr/governance/ADR-970-automation-log-integration-standard.md");
        
        content.Should().Contain("CI 必须自动生成结构化日志", ciLogGenerationMessage);
    }

}
