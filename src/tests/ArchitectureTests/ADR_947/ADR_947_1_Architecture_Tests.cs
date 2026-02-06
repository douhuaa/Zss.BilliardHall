namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_947;


/// <summary>
/// ADR-947_1: 唯一顶级关系区原则（Rule）
/// 验证每个 ADR 必须且仅能包含一个顶级关系声明章节
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-947_1_1: 唯一顶级关系区
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md
/// </summary>
public sealed class ADR_947_1_Architecture_Tests
{
    /// <summary>
    /// ADR-947_1_1: 唯一顶级关系区
    /// 验证每个 ADR 文档必须且仅能包含一个 ## Relationships（关系声明）顶级标题（§ADR-947_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-947_1_1: ADR 必须且仅能包含一个顶级关系声明章节")]
    public void ADR_947_1_1_ADR_Must_Have_Unique_Top_Level_Relationships_Section()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = FileSystemTestHelper.GetAbsolutePath("docs/adr");

        // 获取所有 ADR 文档（使用 AdrFileFilter 自动排除非 ADR 文档）
        var adrFiles = AdrFileFilter.GetAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var fileName = Path.GetFileName(adrFile);
            var content = File.ReadAllText(adrFile);
            var lines = content.Split('\n');

            // 统计顶级 ## Relationships 章节的数量
            var relationshipSectionCount = lines
                .Count(line => line.TrimStart().StartsWith("## Relationships") ||
                               line.TrimStart().StartsWith("## 关系声明"));

            if (relationshipSectionCount == 0)
            {
                violations.Add($"{fileName}: 缺少 Relationships 章节");
            }
            else if (relationshipSectionCount > 1)
            {
                violations.Add($"{fileName}: 包含 {relationshipSectionCount} 个顶级 Relationships 章节（应该只有 1 个）");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-947_1_1：以下 ADR 文档的 Relationships 章节不符合唯一性要求：\n{string.Join("\n", violations)}");
    }

}
