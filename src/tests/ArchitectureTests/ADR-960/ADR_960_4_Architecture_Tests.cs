namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_4: Onboarding 文档维护与失效治理
/// 验证 Onboarding 文档的维护和失效治理机制
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_4_1: 绑定 ADR 演进
/// - ADR-960_4_2: 失效处理
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_4_Architecture_Tests
{
    /// <summary>
    /// ADR-960_4_1: 绑定 ADR 演进
    /// 验证 Onboarding 维护规则已定义（§ADR-960_4_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_4_1: Onboarding 维护规则必须已定义")]
    public void ADR_960_4_1_Onboarding_Maintenance_Rules_Must_Be_Defined()
    {
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr960Path);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_4_1",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "创建 ADR-960 文档",
                "在文档中定义 Onboarding 维护规则"
            },
            adrReference: TestConstants.Adr960Path);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        // 检查维护规则关键词
        var maintenanceKeywords = new[]
        {
            "绑定 ADR 演进",
            "必须评估是否更新"
        };

        var hasMaintenanceRules = FileSystemTestHelper.FileContainsAnyKeyword(
            adr960Path,
            maintenanceKeywords,
            ignoreCase: true);

        var maintenanceMessage = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_4_1",
            summary: "ADR-960 必须定义 Onboarding 维护规则",
            currentState: "文档中未找到维护规则（应包含'绑定 ADR 演进'或'必须评估是否更新'）",
            remediationSteps: new[]
            {
                "定义维护触发条件：当新 ADR 被采纳时",
                "定义维护触发条件：当 ADR 结构发生重大调整时",
                "定义维护触发条件：当新的核心 Case 出现时",
                "定义审计频率：至少每半年一次有效性审计"
            },
            adrReference: TestConstants.Adr960Path,
            includeClauseReference: true);

        hasMaintenanceRules.Should().BeTrue(maintenanceMessage);

        // 检查审计频率
        var auditKeywords = new[] { "半年", "6 个月" };
        var hasSemiAnnualAudit = FileSystemTestHelper.FileContainsAnyKeyword(
            adr960Path,
            auditKeywords,
            ignoreCase: true);

        var auditMessage = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_4_1",
            summary: "ADR-960 必须定义审计频率（至少每半年一次）",
            currentState: "文档中未找到审计频率定义（应包含'半年'或'6 个月'）",
            remediationSteps: new[]
            {
                "明确定义审计频率：至少每半年一次",
                "说明审计的内容和标准"
            },
            adrReference: TestConstants.Adr960Path,
            includeClauseReference: true);

        hasSemiAnnualAudit.Should().BeTrue(auditMessage);
    }

    /// <summary>
    /// ADR-960_4_2: 失效处理
    /// 验证 Onboarding 失效处理机制已定义（§ADR-960_4_2）
    /// </summary>
    [Fact(DisplayName = "ADR-960_4_2: Onboarding 失效处理机制必须已定义")]
    public void ADR_960_4_2_Onboarding_Deprecation_Mechanism_Must_Be_Defined()
    {
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr960Path);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_4_2",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "创建 ADR-960 文档",
                "在文档中定义 Onboarding 失效处理机制"
            },
            adrReference: TestConstants.Adr960Path);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        // 检查失效处理关键词
        var deprecationKeywords = new[]
        {
            "失效处理",
            "内容误导"
        };

        var hasDeprecationProcess = FileSystemTestHelper.FileContainsAnyKeyword(
            adr960Path,
            deprecationKeywords,
            ignoreCase: true);

        var deprecationMessage = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_4_2",
            summary: "ADR-960 必须定义 Onboarding 失效处理流程",
            currentState: "文档中未找到失效处理流程（应包含'失效处理'或'内容误导'）",
            remediationSteps: new[]
            {
                "定义失效处理流程：发现内容误导 → 立即修复",
                "定义应急措施：无法及时修复 → 标记 [可能过时]",
                "禁止长期错误但'懒得改'的情况",
                "建立内容审查和反馈机制"
            },
            adrReference: TestConstants.Adr960Path,
            includeClauseReference: true);

        hasDeprecationProcess.Should().BeTrue(deprecationMessage);

        // 验证禁止过时内容长期存在
        var outdatedKeywords = new[] { "不允许", "过时", "废弃" };
        var forbidsOutdatedContent = FileSystemTestHelper.FileContainsAllKeywords(
            adr960Path,
            new[] { "不允许" },
            ignoreCase: true) &&
            FileSystemTestHelper.FileContainsAnyKeyword(
                adr960Path,
                new[] { "过时", "废弃" },
                ignoreCase: true);

        var outdatedMessage = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_4_2",
            summary: "ADR-960 必须明确禁止过时内容长期存在",
            currentState: "文档中未找到明确的禁止条款（应同时包含'不允许'和'过时'或'废弃'）",
            remediationSteps: new[]
            {
                "明确声明不允许过时内容长期存在",
                "定义内容审查和更新的责任人",
                "建立反馈和报告机制"
            },
            adrReference: TestConstants.Adr960Path,
            includeClauseReference: true);

        forbidsOutdatedContent.Should().BeTrue(outdatedMessage);
    }
}
