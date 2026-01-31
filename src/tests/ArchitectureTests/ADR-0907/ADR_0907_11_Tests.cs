using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.11: L1 阻断 / L2 告警策略执行
/// 验证执行级别分类是否有文档支持（§4.3）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_11_Tests
{
    [Fact(DisplayName = "ADR-907.11: 执行级别策略必须有文档支持")]
    public void Enforcement_Levels_Must_Be_Documented()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 检查 ADR-905（执行级别分类）是否存在
        var adr905Path = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrDocsPath, "governance", "ADR-905-enforcement-level-classification.md");
        
        File.Exists(adr905Path).Should().BeTrue(
            $"❌ ADR-907.11 违规：缺少执行级别分类文档\n\n" +
            $"预期路径：{adr905Path}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ADR-905 文档定义 L1/L2 执行级别\n" +
            $"  2. L1：失败即阻断 CI / 合并 / 部署\n" +
            $"  3. L2：失败记录告警，进入人工 Code Review\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.3");
    }
}
