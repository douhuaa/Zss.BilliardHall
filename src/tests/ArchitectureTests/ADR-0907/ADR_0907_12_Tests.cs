using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.12: 破例与偿还机制记录
/// 验证破例机制必须有文档支持（§4.4）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_12_Tests
{
    [Fact(DisplayName = "ADR-907.12: 破例机制必须有文档支持")]
    public void Exception_Mechanism_Must_Be_Documented()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 检查 ADR-0000（架构测试宪法）是否存在并包含破例机制
        var adr0000Path = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrDocsPath, "governance", "ADR-0000-architecture-tests.md");
        
        File.Exists(adr0000Path).Should().BeTrue(
            $"❌ ADR-907.12 违规：缺少架构测试宪法文档\n\n" +
            $"预期路径：{adr0000Path}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ADR-0000 定义破例与偿还机制\n" +
            $"  2. 破例必须记录：ADR 编号、测试类/方法、原因、到期时间\n" +
            $"  3. 建立偿还追踪机制\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.4");
    }
}
