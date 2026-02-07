namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_2: Onboarding 与其他文档的分离边界
/// 验证 Onboarding 文档与其他文档类型的分离边界
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_2_1: 内容类型限制
/// - ADR-960_2_2: 核心原则
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_2_Architecture_Tests
{
    /// <summary>
    /// ADR-960_2_1: 内容类型限制
    /// 验证 Onboarding 文档不包含禁止的内容类型（§ADR-960_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_2_1: Onboarding 必须遵循内容类型限制")]
    public void ADR_960_2_1_Onboarding_Must_Follow_Content_Type_Restrictions()
    {
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_2_1",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "创建 ADR-960 文档",
                "在文档中定义 Onboarding 的内容类型限制表"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        // 验证定义了内容类型限制表格
        var hasContentTypeTable = FileSystemTestHelper.FileContainsTable(
            adr960Path, 
            "是否允许出现在 Onboarding");

        var tableMessage = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_2_1",
            summary: "ADR-960 必须定义 Onboarding 的内容类型限制表",
            currentState: "文档中未找到内容类型限制表（应包含'是否允许出现在 Onboarding'列）",
            remediationSteps: new[]
            {
                "在 ADR-960 中添加内容类型限制表格",
                "表格应包含'内容类型'和'是否允许出现在 Onboarding'列",
                "明确列出允许和禁止的内容类型"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960,
            includeClauseReference: true);

        hasContentTypeTable.Should().BeTrue(tableMessage);

        // 验证表格包含关键内容类型限制
        var missingContentTypes = FileSystemTestHelper.GetMissingKeywords(
            adr960Path,
            ArchitectureTestSpecification.Onboarding.ProhibitedContent,
            ignoreCase: true);

        var contentTypeMessage = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-960_2_1",
            summary: "内容类型限制表缺少必需的内容类型",
            failingTypes: missingContentTypes,
            remediationSteps: new[]
            {
                "在内容类型表中添加缺失的内容类型",
                "确保所有禁止的内容类型都有明确的限制说明"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        missingContentTypes.Should().BeEmpty(contentTypeMessage);
    }

    /// <summary>
    /// ADR-960_2_2: 核心原则
    /// 验证 Onboarding 文档遵循三个核心问题原则（§ADR-960_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-960_2_2: Onboarding 必须遵循三个核心问题原则")]
    public void ADR_960_2_2_Onboarding_Must_Follow_Core_Principles()
    {
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_2_2",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "创建 ADR-960 文档",
                "在文档中定义 Onboarding 的三个核心问题"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        // 验证定义了三个核心问题
        var missingQuestions = FileSystemTestHelper.GetMissingKeywords(
            adr960Path,
            ArchitectureTestSpecification.Onboarding.CoreQuestions,
            ignoreCase: true);

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-960_2_2",
            summary: "ADR-960 必须定义 Onboarding 的三个核心问题",
            failingTypes: missingQuestions.Select(q => $"缺少核心问题：{q}"),
            remediationSteps: new[]
            {
                "在 ADR-960 中添加三个核心问题的定义",
                "核心问题包括：我是谁（这个项目是什么）",
                "我先看什么（新人应该先阅读哪些文档）",
                "我下一步去哪（后续学习路径）"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        missingQuestions.Should().BeEmpty(message);
    }
}
