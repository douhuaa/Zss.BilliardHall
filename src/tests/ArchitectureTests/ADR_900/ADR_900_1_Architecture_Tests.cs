namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_900;

using Zss.BilliardHall.Tests.ArchitectureTests.Adr;

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

        // 验证 ADR 目录存在
        Directory.Exists(adrDirectory).Should().BeTrue(
            $"❌ ADR-900_1_1 违规：ADR 文档目录不存在\n\n" +
            $"预期路径：{adrDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR 文档位于 docs/adr/ 目录\n" +
            $"  2. ADR 正文是架构裁决的唯一依据\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §1.1");

        // 获取所有正式 ADR 文件（使用 AdrFileFilter 自动排除非 ADR 文档）
        var adrFiles = AdrFileFilter.GetAdrFiles(adrDirectory).ToArray();

        adrFiles.Should().NotBeEmpty("❌ ADR-900_1_1 违规：未找到任何 ADR 文档文件");

        // 验证每个 ADR 都声明正文为唯一裁决依据
        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否包含唯一裁决声明
            content.Should().Contain("唯一裁决",
                $"❌ ADR-900_1_1 违规：{fileName} 未明确声明 ADR 正文为唯一裁决依据\n\n" +
                $"修复建议：\n" +
                $"  在 ADR 的 Decision 章节开头添加：\n" +
                $"  '⚠️ 本节为唯一裁决来源，所有条款具备执行级别。'\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §1.1");
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

        // 验证包含架构违规判定标准
        content.Should().Contain("架构违规",
            $"❌ ADR-900_1_2 违规：ADR-900 未定义架构违规判定原则\n\n" +
            $"修复建议：\n" +
            $"  在 ADR-900 中明确架构违规的判定条件\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §1.2");

        // 验证包含 CI 阻断机制
        content.Should().Contain("CI 阻断",
            $"❌ ADR-900_1_2 违规：ADR-900 未定义 CI 阻断机制\n\n" +
            $"修复建议：\n" +
            $"  在 ADR-900 中明确 CI 阻断条件\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §1.2");
    }
}
