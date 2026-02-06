namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_2: 自动化检测机制
/// 验证质量检测必须自动化执行的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_2_1: 质量检测必须自动化执行
/// - ADR-975_2_2: 链接有效性检测
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-975_2_1: 质量检测必须自动化执行")]
    public void ADR_975_2_1_Quality_Checks_Must_Be_Automated()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        var message = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_2_1",
            filePath: adr975Path,
            missingContent: "自动化执行",
            remediationSteps: new[]
            {
                "在 ADR-975 文档中明确要求质量检测必须自动化执行",
                "说明自动化执行的具体方式（CI/CD 集成）",
                "定义自动化检测的触发条件"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        content.Should().Contain("自动化执行", message);
    }

    [Fact(DisplayName = "ADR-975_2_2: 必须包含链接有效性检测")]
    public void ADR_975_2_2_Link_Validity_Check_Must_Be_Included()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        var message = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_2_2",
            filePath: adr975Path,
            missingContent: "链接有效性检测",
            remediationSteps: new[]
            {
                "在 ADR-975 文档中添加链接有效性检测要求",
                "说明链接检测的范围和方式",
                "定义链接失效时的处理流程"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        content.Should().Contain("链接有效性检测", message);
    }

}
