namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_965;

/// <summary>
/// ADR-965_2: 学习路径可视化
/// 验证 Onboarding 学习路径的可视化规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-965_2_1: 必须包含可视化学习路径图
/// - ADR-965_2_2: 路径图位置验证
/// - ADR-965_2_3: 可视化格式（Mermaid）验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md
/// - version: 2.0
/// </summary>
public sealed class ADR_965_2_Architecture_Tests
{
    /// <summary>
    /// ADR-965_2_1: 可视化路径图存在性验证
    /// 验证 ADR-965 定义了可视化学习路径图（§ADR-965_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-965_2_1: 必须包含可视化学习路径图")]
    public void ADR_965_2_1_Must_Include_Visual_Learning_Path()
    {
        var adr965Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr965Path);

        // 验证包含可视化路径图的说明
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_2_1",
            filePath: adr965Path,
            missingContent: "可视化学习路径图",
            remediationSteps: new[]
            {
                "在 ADR-965 中添加关于可视化学习路径图的说明",
                "说明路径图的目的和作用",
                "提供路径图的创建指南"
            },
            adrReference: TestConstants.Adr965Path);

        FileSystemTestHelper.AssertFileContains(adr965Path, "可视化学习路径图", missingMessage);
    }

    /// <summary>
    /// ADR-965_2_2: 路径图位置验证
    /// 验证定义了路径图的标准位置（§ADR-965_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-965_2_2: 必须定义路径图位置")]
    public void ADR_965_2_2_Path_Location_Must_Be_Defined()
    {
        var adr965Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr965Path);

        // 验证定义了路径图位置
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_2_2",
            filePath: adr965Path,
            missingContent: "docs/onboarding/README.md",
            remediationSteps: new[]
            {
                "在 ADR-965 中明确定义路径图位置为 docs/onboarding/README.md",
                "说明路径图位置选择的原因",
                "提供路径图访问方式"
            },
            adrReference: TestConstants.Adr965Path);

        FileSystemTestHelper.AssertFileContains(adr965Path, "docs/onboarding/README.md", missingMessage);
    }

    /// <summary>
    /// ADR-965_2_3: 可视化格式验证
    /// 验证定义了使用 Mermaid 图表格式（§ADR-965_2_3）
    /// </summary>
    [Fact(DisplayName = "ADR-965_2_3: 必须使用 Mermaid 格式")]
    public void ADR_965_2_3_Must_Use_Mermaid_Format()
    {
        var adr965Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr965Path);

        // 验证定义了 Mermaid 格式
        var missingMermaidMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_2_3",
            filePath: adr965Path,
            missingContent: "Mermaid",
            remediationSteps: new[]
            {
                "在 ADR-965 中说明使用 Mermaid 图表格式",
                "解释选择 Mermaid 的原因",
                "提供 Mermaid 使用指南"
            },
            adrReference: TestConstants.Adr965Path);

        FileSystemTestHelper.AssertFileContains(adr965Path, "Mermaid", missingMermaidMessage);

        // 验证包含 Mermaid 代码块示例
        var missingExampleMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_2_3",
            filePath: adr965Path,
            missingContent: "```mermaid",
            remediationSteps: new[]
            {
                "在 ADR-965 中添加 Mermaid 图表示例",
                "提供完整的 Mermaid 代码块",
                "确保示例清晰易懂"
            },
            adrReference: TestConstants.Adr965Path);

        FileSystemTestHelper.AssertFileContains(adr965Path, "```mermaid", missingExampleMessage);
    }
}