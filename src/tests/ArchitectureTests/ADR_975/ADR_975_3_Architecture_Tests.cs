namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_3: 定期报告生成
/// 验证必须每月生成文档质量报告的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_3_1: 必须每月生成文档质量报告
/// - ADR-975_3_2: 报告位置验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-975_3_1: 必须每月生成文档质量报告")]
    public void ADR_975_3_1_Monthly_Quality_Report_Must_Be_Generated()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        var message = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_3_1",
            filePath: adr975Path,
            missingContent: "每月生成",
            remediationSteps: new[]
            {
                "在 ADR-975 中添加每月生成文档质量报告的要求",
                "说明报告生成的具体时间点（每月第一天）",
                "定义报告的负责人和审查流程"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        content.Should().Contain("每月生成", message);
    }

    [Fact(DisplayName = "ADR-975_3_2: 必须定义报告位置")]
    public void ADR_975_3_2_Report_Location_Must_Be_Defined()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        var message = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_3_2",
            filePath: adr975Path,
            missingContent: "docs/reports/quality",
            remediationSteps: new[]
            {
                "在 ADR-975 中定义报告的标准位置为 docs/reports/quality",
                "说明报告文件的命名格式",
                "确保报告位置被纳入版本控制"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        content.Should().Contain("docs/reports/quality", message);
    }

}
