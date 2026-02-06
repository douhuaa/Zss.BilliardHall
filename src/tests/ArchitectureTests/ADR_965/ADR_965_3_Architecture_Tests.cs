namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_965;

/// <summary>
/// ADR-965_3: 进度跟踪机制
/// 验证 Onboarding 进度跟踪机制的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-965_3_1: 必须实时追踪 Onboarding 进度
/// - ADR-965_3_2: GitHub Issue 进度条验证
/// - ADR-965_3_3: Project Board 集成验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md
/// - version: 2.0
/// </summary>
public sealed class ADR_965_3_Architecture_Tests
{
    /// <summary>
    /// ADR-965_3_1: 进度跟踪机制存在性验证
    /// 验证 ADR-965 定义了实时进度跟踪机制（§ADR-965_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-965_3_1: 必须实时追踪 Onboarding 进度")]
    public void ADR_965_3_1_Must_Track_Progress_Realtime()
    {
        var adr965Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = FileSystemTestHelper.ReadFileContent(adr965Path);

        // 验证包含进度跟踪机制的说明
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_3_1",
            filePath: adr965Path,
            missingContent: "实时追踪",
            remediationSteps: new[]
            {
                "在 ADR-965 中添加实时追踪 Onboarding 进度的说明",
                "定义进度跟踪机制和实施方法"
            },
            adrReference: "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        content.Should().Contain("实时追踪", missingMessage);
    }

    /// <summary>
    /// ADR-965_3_2: GitHub Issue 进度条验证
    /// 验证定义了 GitHub Issue 进度条机制（§ADR-965_3_2）
    /// </summary>
    [Fact(DisplayName = "ADR-965_3_2: 必须使用 GitHub Issue 进度条")]
    public void ADR_965_3_2_Must_Use_GitHub_Issue_Progress()
    {
        var adr965Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = FileSystemTestHelper.ReadFileContent(adr965Path);

        // 验证定义了 GitHub Issue 进度条
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_3_2",
            filePath: adr965Path,
            missingContent: "GitHub Issue 进度条",
            remediationSteps: new[]
            {
                "在 ADR-965 中定义 GitHub Issue 进度条机制",
                "说明如何使用 Issue 进度条追踪学习进度"
            },
            adrReference: "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        content.Should().Contain("GitHub Issue 进度条", missingMessage);
    }

    /// <summary>
    /// ADR-965_3_3: Project Board 集成验证
    /// 验证定义了 Project Board 集成（§ADR-965_3_3）
    /// </summary>
    [Fact(DisplayName = "ADR-965_3_3: 必须集成 Project Board")]
    public void ADR_965_3_3_Must_Integrate_Project_Board()
    {
        var adr965Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = FileSystemTestHelper.ReadFileContent(adr965Path);

        // 验证定义了 Project Board 集成
        var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-965_3_3",
            filePath: adr965Path,
            missingContent: "Project Board",
            remediationSteps: new[]
            {
                "在 ADR-965 中定义 Project Board 集成",
                "说明如何将 Onboarding 任务整合到 Project Board"
            },
            adrReference: "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        content.Should().Contain("Project Board", missingMessage);
    }

}
