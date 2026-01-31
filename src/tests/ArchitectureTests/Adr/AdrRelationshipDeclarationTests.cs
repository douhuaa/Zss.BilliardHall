using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940: ADR 关系与溯源管理
/// 验证每个 ADR 文档都包含关系声明章节
/// </summary>
public sealed class AdrRelationshipDeclarationTests
{
    private readonly IReadOnlyList<AdrDocument> _adrs;

    public AdrRelationshipDeclarationTests()
    {
        var repoRoot = FindRepositoryRoot() 
            ?? throw new InvalidOperationException("未找到仓库根目录（无法定位 docs/adr 或 .git）");
        
        var adrPath = Path.Combine(repoRoot, "docs", "adr");
        var repo = new AdrRepository(adrPath);
        
        _adrs = repo.LoadAll();
    }

    /// <summary>
    /// ADR-940.1: 每个 ADR 必须包含关系声明章节
    /// </summary>
    [Fact(DisplayName = "ADR-940.1: 所有 ADR 必须包含关系声明章节")]
    public void All_ADRs_Must_Have_Relationship_Section()
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

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        return null;
    }
}
