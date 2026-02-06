namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_900;


/// <summary>
/// ADR-900_1: 架构裁决权威性
/// 验证 ADR 正文作为唯一裁决依据的相关规则
///
/// 测试覆盖映射（严格遵循 ADR-900 v4.0 Rule/Clause 体系）：
/// - ADR-900_1_1: 审判权唯一性 → ADR_900_1_1_ADR_Text_Is_Sole_Authority
/// - ADR-900_1_2: 架构违规的判定原则 → ADR_900_1_2_Architecture_Violation_Criteria
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-900-architecture-tests.md
/// </summary>
public sealed class ADR_900_1_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR-900_1_1: 审判权唯一性
    /// 验证 ADR 正文是唯一裁决依据，README、Prompt、示例、脚本不具备裁决权
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_1: ADR 正文必须是唯一裁决依据")]
    public void ADR_900_1_1_ADR_Text_Is_Sole_Authority()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        var dirMessage = AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
            ruleId: "ADR-900_1_1",
            directoryPath: adrDirectory,
            directoryDescription: "ADR 文档目录",
            remediationSteps: new[]
            {
                "确保 ADR 文档位于 docs/adr/ 目录",
                "ADR 正文是架构裁决的唯一依据"
            },
            adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
        Directory.Exists(adrDirectory).Should().BeTrue(dirMessage);

        // 获取所有正式 ADR 文件（使用 AdrFileFilter 自动排除非 ADR 文档）
        var adrFiles = AdrFileFilter.GetAdrFiles(adrDirectory).ToArray();

        adrFiles.Should().NotBeEmpty("❌ ADR-900_1_1 违规：未找到任何 ADR 文档文件");

        // 验证每个 ADR 都声明正文为唯一裁决依据
        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            var contentMessage = AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-900_1_1",
                filePath: adrFile,
                missingContent: "唯一裁决",
                remediationSteps: new[]
                {
                    "在 ADR 的 Decision 章节开头添加：",
                    "'⚠️ 本节为唯一裁决来源，所有条款具备执行级别。'"
                },
                adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
            // 检查是否包含唯一裁决声明
            content.Should().Contain("唯一裁决", contentMessage);
        }
    }

    /// <summary>
    /// ADR-900_1_2: 架构违规的判定原则
    /// 验证架构违规的判定标准：必须架构测试失败、CI Gate 失败等
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_2: 架构违规判定必须基于明确标准")]
    public void ADR_900_1_2_Architecture_Violation_Criteria()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        // 使用 AdrFileFilter 统一过滤 ADR 文件
        var adrFiles = AdrFileFilter.GetAdrFiles(adrDirectory).ToArray();

        // 验证 ADR-900 本身定义了违规判定原则
        var adr900File = adrFiles.FirstOrDefault(f => Path.GetFileName(f).StartsWith("ADR-900"));
        adr900File.Should().NotBeNull("❌ ADR-900_1_2 违规：未找到 ADR-900 文档");

        var content = File.ReadAllText(adr900File);

        var violationMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-900_1_2",
            filePath: adr900File,
            missingContent: "架构违规",
            remediationSteps: new[]
            {
                "在 ADR-900 中明确架构违规的判定条件"
            },
            adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
        // 验证包含架构违规判定标准
        content.Should().Contain("架构违规", violationMessage);

        var ciMessage = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-900_1_2",
            filePath: adr900File,
            missingContent: "CI 阻断",
            remediationSteps: new[]
            {
                "在 ADR-900 中明确 CI 阻断条件"
            },
            adrReference: "docs/adr/governance/ADR-900-architecture-tests.md");
        // 验证包含 CI 阻断机制
        content.Should().Contain("CI 阻断", ciMessage);
    }
}
