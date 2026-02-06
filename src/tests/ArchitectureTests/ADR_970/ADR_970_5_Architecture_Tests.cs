namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_5: 日志分发与访问
/// 验证日志必须易于访问的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_5_1: 日志必须易于访问
/// - ADR-970_5_2: 访问方式验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_5_Architecture_Tests
{
    [Fact(DisplayName = "ADR-970_5_1: 日志必须易于访问")]
    public void ADR_970_5_1_Logs_Must_Be_Accessible()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        var accessibilityMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-970_5_1",
            filePath: adr970Path,
            missingContent: "易于访问",
            remediationSteps: new[]
            {
                "在 ADR-970 中要求日志易于访问",
                "定义可访问性要求"
            },
            adrReference: "docs/adr/governance/ADR-970-automation-log-integration-standard.md");
        
        content.Should().Contain("易于访问", accessibilityMessage);
    }

    [Fact(DisplayName = "ADR-970_5_2: 必须定义访问方式")]
    public void ADR_970_5_2_Access_Methods_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        var accessMethodsMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-970_5_2",
            filePath: adr970Path,
            missingContent: "访问方式",
            remediationSteps: new[]
            {
                "在 ADR-970 中定义访问方式",
                "说明如何获取和查看日志"
            },
            adrReference: "docs/adr/governance/ADR-970-automation-log-integration-standard.md");
        
        content.Should().Contain("访问方式", accessMethodsMessage);
    }

}
