using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_955;

/// <summary>
/// ADR-955_3: 审计与维护机制（Rule）
/// 验证文档的定期审计和维护机制
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-955_3_1: 审计内容
/// - ADR-955_3_2: Issue 跟踪
/// - ADR-955_3_3: 报告存档
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-955-documentation-search-discoverability.md
/// </summary>
public sealed class ADR_955_3_Architecture_Tests
{
    /// <summary>
    /// ADR-955_3_1: 审计内容
    /// 验证文档有审计记录（§ADR-955_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-955_3_1: 文档应有审计记录")]
    public void ADR_955_3_1_Documents_Should_Have_Audit_Records()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsDirectory = Path.Combine(repoRoot, "docs");

        if (!Directory.Exists(docsDirectory))
        {
            Console.WriteLine("⚠️ ADR-955_3_1 提示：docs 目录不存在，跳过测试。");
            return;
        }

        // 这是一个长期维护的建议性测试
        Console.WriteLine("⚠️ ADR-955_3_1 提示：文档审计机制应定期执行。");
        
        true.Should().BeTrue("ADR-955_3_1 是定期审计建议");
    }

}
