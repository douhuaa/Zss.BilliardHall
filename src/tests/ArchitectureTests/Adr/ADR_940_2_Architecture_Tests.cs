using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940_2: ADR 关系声明章节规范（Rule）
/// 验证每个 ADR 文档都包含关系声明章节
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-940_2_1: 所有 ADR 必须包含关系声明章节
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-940-adr-relationship-and-traceability.md
/// </summary>
public sealed class ADR_940_2_Architecture_Tests
{
    private readonly IReadOnlyList<AdrDocument> _adrs;

    public ADR_940_2_Architecture_Tests()
    {
        var repoRoot = TestEnvironment.RepositoryRoot 
            ?? throw new InvalidOperationException("未找到仓库根目录（无法定位 docs/adr 或 .git）");
        
        var adrPath = Path.Combine(repoRoot, "docs", "adr");
        var repo = new AdrRepository(adrPath);
        
        _adrs = repo.LoadAll();
    }

    /// <summary>
    /// ADR-940_2_1: 所有 ADR 必须包含关系声明章节
    /// 验证每个 ADR 文档都包含 Relationships 或关系声明章节（§ADR-940_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-940_2_1: 所有 ADR 必须包含关系声明章节")]
    public void ADR_940_2_1_All_ADRs_Must_Have_Relationship_Section()
    {
        var violations = new List<string>();

        foreach (var adr in _adrs)
        {
            // 检查 ADR 文档是否包含 Relationships 章节
            if (!HasRelationshipSection(adr.FilePath))
            {
                violations.Add(
                    $"❌ {adr.Id} 缺少关系声明章节\n" +
                    $"   文件：{adr.FilePath}\n" +
                    $"   修复：添加 ## Relationships（关系声明）章节"
                );
            }
        }

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// 检查文件是否包含关系声明章节
    /// </summary>
    private static bool HasRelationshipSection(string filePath)
    {
        var content = File.ReadAllText(filePath);
        
        // 支持中英文双语格式
        return content.Contains("## Relationships", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("## 关系声明", StringComparison.OrdinalIgnoreCase);
    }
}
