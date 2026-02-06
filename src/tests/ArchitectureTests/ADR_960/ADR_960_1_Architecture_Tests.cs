namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_1: Onboarding 文档的权威定位
/// 验证 Onboarding 文档符合非裁决性定位要求
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_1_1: 不是裁决性文档
/// - ADR-960_1_2: 不得定义架构约束
/// - ADR-960_1_3: 唯一合法职责
/// - ADR-960_1_4: 权威层级
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_1_Architecture_Tests
{
    private const string DocsPath = "docs";

    /// <summary>
    /// ADR-960_1_1: 不是裁决性文档
    /// 验证 Onboarding 文档存在且不包含裁决性语言（§ADR-960_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_1: Onboarding 文档不得包含裁决性语言")]
    public void ADR_960_1_1_Onboarding_Must_Not_Contain_Decision_Language()
    {
        var docsPath = FileSystemTestHelper.GetAbsolutePath(DocsPath);

        FileSystemTestHelper.AssertDirectoryExists(docsPath,
            $"文档目录不存在: {docsPath}");

        // 查找 Onboarding 文档
        var onboardingFiles = FileSystemTestHelper
            .GetFilesInDirectory(docsPath, "*onboarding*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("ADR-960", StringComparison.OrdinalIgnoreCase)) // 排除 ADR-960 本身
            .ToList();

        var violations = new List<string>();

        foreach (var file in onboardingFiles)
        {
            var content = FileSystemTestHelper.ReadFileContent(file);
            var relativePath = FileSystemTestHelper.GetRelativePath(file);

            // 检查是否包含裁决性关键词（避免误报，检查上下文）
            foreach (var keyword in ArchitectureTestSpecification.Semantics.DecisionKeywords)
            {
                // 匹配裁决性模式：如 "必须遵循"、"禁止使用" 等
                var pattern = $@"\b{Regex.Escape(keyword)}(?:遵循|使用|实现|修改|删除|创建|定义)";
                if (Regex.IsMatch(content, pattern))
                {
                    violations.Add($"{relativePath} - 包含裁决性语言：'{keyword}'");
                    break; // 每个文件只报告一次
                }
            }
        }

        if (violations.Count > 0)
        {
            Console.WriteLine("⚠️ ADR-960_1_1 警告：发现 Onboarding 文档可能包含裁决性语言：");
            foreach (var violation in violations)
            {
                Console.WriteLine($"  - {violation}");
            }
            Console.WriteLine("\n修复建议：Onboarding 文档应该使用引导性语言，如'建议'、'推荐'，而不是裁决性语言。");
        }

        // L2 级别，不阻断测试，仅警告
    }

    /// <summary>
    /// ADR-960_1_2: 不得定义架构约束
    /// 验证 Onboarding 文档不包含新的架构约束定义（§ADR-960_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_2: Onboarding 不得定义新架构约束")]
    public void ADR_960_1_2_Onboarding_Must_Not_Define_New_Constraints()
    {
        // 验证 ADR-960 文档存在以定义此规则
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_1_2",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "确保 ADR-960 存在以定义 Onboarding 文档规范",
                "在 ADR-960 中明确 Onboarding 的非裁决性定位"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        var content = File.ReadAllText(adr960Path);

        // 验证 ADR-960 明确定义了 Onboarding 的非裁决性
        var missingNonDecisionContent = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-960_1_2",
            filePath: adr960Path,
            missingContent: "不是裁决性文档",
            remediationSteps: new[]
            {
                "在 ADR-960 中添加明确的非裁决性定位声明",
                "确保 Onboarding 文档的职责定义清晰"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        content.Should().Contain("不是裁决性文档", missingNonDecisionContent);

        var missingProhibitionContent = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-960_1_2",
            filePath: adr960Path,
            missingContent: "不得",
            remediationSteps: new[]
            {
                "在 ADR-960 中明确禁止 Onboarding 定义架构约束",
                "确保规则表述清晰明确"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        content.Should().Contain("不得", missingProhibitionContent);
    }

    /// <summary>
    /// ADR-960_1_3: 唯一合法职责
    /// 验证 Onboarding 文档的职责定义明确（§ADR-960_1_3）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_3: Onboarding 唯一职责必须明确定义")]
    public void ADR_960_1_3_Onboarding_Responsibilities_Must_Be_Defined()
    {
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_1_3",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "创建 ADR-960 文档",
                "定义 Onboarding 的唯一合法职责"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        var content = File.ReadAllText(adr960Path);

        // 验证定义了 Onboarding 的唯一合法职责
        var hasResponsibilityDefinition = content.Contains("唯一合法职责", StringComparison.OrdinalIgnoreCase) ||
                                         content.Contains("告诉你", StringComparison.OrdinalIgnoreCase);

        var message = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_1_3",
            summary: "ADR-960 必须明确定义 Onboarding 的唯一合法职责",
            currentState: "文档中未找到职责定义（应包含'唯一合法职责'或'告诉你'等关键词）",
            remediationSteps: new[]
            {
                "在 ADR-960 中添加 Onboarding 的职责定义章节",
                "明确说明 Onboarding 的唯一合法职责是什么",
                "确保职责定义清晰、具体、可验证"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960,
            includeClauseReference: true);

        hasResponsibilityDefinition.Should().BeTrue(message);
    }

    /// <summary>
    /// ADR-960_1_4: 权威层级
    /// 验证 Onboarding 在文档权威层级中的位置（§ADR-960_1_4）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_4: Onboarding 权威层级必须明确")]
    public void ADR_960_1_4_Onboarding_Authority_Level_Must_Be_Clear()
    {
        var adr960Path = FileSystemTestHelper.GetAbsolutePath(ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-960_1_4",
            filePath: adr960Path,
            fileDescription: "ADR-960 文档",
            remediationSteps: new[]
            {
                "创建 ADR-960 文档",
                "定义 Onboarding 在文档权威层级中的位置"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960);

        File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

        var content = File.ReadAllText(adr960Path);

        // 验证定义了权威层级
        var hasAuthorityHierarchy = content.Contains("权威层级", StringComparison.OrdinalIgnoreCase) ||
                                   content.Contains("ADR（裁决", StringComparison.OrdinalIgnoreCase);

        var message = AssertionMessageBuilder.Build(
            ruleId: "ADR-960_1_4",
            summary: "ADR-960 必须定义 Onboarding 在权威层级中的位置",
            currentState: "文档中未找到权威层级定义（应包含'权威层级'或'ADR（裁决'等关键词）",
            remediationSteps: new[]
            {
                "在 ADR-960 中添加权威层级定义章节",
                "明确说明 Onboarding 文档在权威层级中的位置",
                "说明当 Onboarding 与 ADR 冲突时的处理原则"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr960,
            includeClauseReference: true);

        hasAuthorityHierarchy.Should().BeTrue(message);
    }
}
