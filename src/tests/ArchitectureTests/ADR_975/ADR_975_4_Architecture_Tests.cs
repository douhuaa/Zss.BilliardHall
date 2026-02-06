namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_4: 质量阈值与阻断策略
/// 验证最低质量阈值和阻断策略的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_4_1: 最低质量阈值验证
/// - ADR-975_4_2: 硬失败项验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-975_4_1: 必须定义最低质量阈值")]
    public void ADR_975_4_1_Minimum_Quality_Threshold_Must_Be_Defined()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        var message = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_4_1",
            filePath: adr975Path,
            missingContent: "≥ 85%",
            remediationSteps: new[]
            {
                "在 ADR-975 中定义最低质量阈值为 85%",
                "说明低于阈值时的处理措施",
                "定义质量评分的计算方法"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        content.Should().Contain("≥ 85%", message);
    }

    [Fact(DisplayName = "ADR-975_4_2: 必须定义硬失败项")]
    public void ADR_975_4_2_Hard_Failure_Items_Must_Be_Defined()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        var hardFailureMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_4_2",
            filePath: adr975Path,
            missingContent: "硬失败项",
            remediationSteps: new[]
            {
                "在 ADR-975 中定义硬失败项清单",
                "说明硬失败项的处理策略（必须立即修复）",
                "定义硬失败项的识别标准"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        content.Should().Contain("硬失败项", hardFailureMessage);

        var linkMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-975_4_2",
            filePath: adr975Path,
            missingContent: "链接失效",
            remediationSteps: new[]
            {
                "确保硬失败项包含'链接失效'",
                "说明链接失效的检测机制",
                "定义链接失效的修复时限"
            },
            adrReference: "docs/adr/governance/ADR-975-documentation-quality-monitoring.md");
        // 验证包含关键硬失败项
        content.Should().Contain("链接失效", linkMessage);
    }

}
