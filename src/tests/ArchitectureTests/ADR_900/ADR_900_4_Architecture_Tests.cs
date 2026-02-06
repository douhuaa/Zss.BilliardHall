namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_900;

/// <summary>
/// ADR-900_4: 冲突裁决优先级
/// 验证架构规则冲突时的裁决优先级顺序
///
/// 测试覆盖映射（严格遵循 ADR-900 v4.0 Rule/Clause 体系）：
/// - ADR-900_4_1: 裁决优先级顺序 → ADR_900_4_1_Conflict_Resolution_Priorities
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-900-architecture-tests.md
/// </summary>
public sealed class ADR_900_4_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR-900_4_1: 裁决优先级顺序
    /// 验证 ADR-900 定义了冲突裁决的优先级顺序
    /// </summary>
    [Fact(DisplayName = "ADR-900_4_1: 必须定义冲突裁决优先级")]
    public void ADR_900_4_1_Conflict_Resolution_Priorities()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr900File = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-900-architecture-tests.md");

        // 验证 ADR-900 文件存在
        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-900_4_1",
            filePath: adr900File,
            fileDescription: "ADR-900 文档",
            remediationSteps: new[]
            {
                "确保 ADR-900 文档位于正确位置",
                "创建冲突裁决优先级定义"
            },
            adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
        
        File.Exists(adr900File).Should().BeTrue(fileNotFoundMessage);

        var content = FileSystemTestHelper.ReadFileContent(adr900File);

        // 验证包含优先级顺序定义
        var priorityMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-900_4_1",
            filePath: adr900File,
            missingContent: "优先级",
            remediationSteps: new[]
            {
                "在 ADR-900 中定义冲突裁决的优先级顺序",
                "明确优先级：安全 > 稳定性 > 生命周期 > 结构 > 流程"
            },
            adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
        
        content.Should().Contain("优先级", priorityMessage);

        // 验证包含具体优先级列表
        var priorityItems = new[] { "安全", "稳定性", "生命周期", "结构一致性", "流程" };
        foreach (var item in priorityItems)
        {
            var itemMissingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-900_4_1",
                filePath: adr900File,
                missingContent: item,
                remediationSteps: new[]
                {
                    $"在 ADR-900 优先级定义中包含 '{item}' 项",
                    "完善优先级顺序定义"
                },
                adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
            
            content.Should().Contain(item, itemMissingMessage);
        }

        // 验证说明了低优先级规则可以被牺牲
        var sacrificeMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-900_4_1",
            filePath: adr900File,
            missingContent: "牺牲",
            remediationSteps: new[]
            {
                "在 ADR-900 中说明低优先级规则的处理方式",
                "明确低优先级规则可以被临时牺牲，但必须记录破例"
            },
            adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
        
        content.Should().Contain("牺牲", sacrificeMessage);
    }
}
