using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_946;

/// <summary>
/// ADR-946_1: 标题级别语义约束
/// 验证 ADR 文档中标题层级与机器可解析语义之间的强制对应关系
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-946_1_1: 标题级别即语义级别
/// - ADR-946_1_2: 关键语义块标题约束
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md
/// </summary>
public sealed class ADR_946_1_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    
    // 关键语义块标题（必须是 ## 级别且唯一）
    private static readonly string[] KeySemanticHeadings = new[]
    {
        "Relationships",
        "Decision",
        "Enforcement",
        "Glossary"
    };

    /// <summary>
    /// ADR-946_1_1: 标题级别即语义级别
    /// 验证每个 ADR 文件必须且仅有一个 # 标题，语义块使用 ## 级别（§ADR-946_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-946_1_1: ADR 文件必须有且仅有一个 # 标题")]
    public void ADR_946_1_1_ADR_Must_Have_Exactly_One_H1_Title()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrPath = Path.Combine(repoRoot, AdrDocsPath);

        if (!Directory.Exists(adrPath))
        {
            throw new DirectoryNotFoundException($"ADR 目录不存在: {adrPath}");
        }

        var adrFiles = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TIMELINE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("CHECKLIST", StringComparison.OrdinalIgnoreCase))
            .Where(f => Path.GetFileName(f).StartsWith("ADR-", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var lines = content.Split('\n');

            // 计算 # 标题数量（必须在行首，不能在代码块中）
            var h1Count = 0;
            var inCodeBlock = false;

            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                
                // 检测代码块
                if (trimmed.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                // 只在非代码块中统计 # 标题
                if (!inCodeBlock && Regex.IsMatch(line, @"^#\s+"))
                {
                    h1Count++;
                }
            }

            if (h1Count != 1)
            {
                violations.Add($"{Path.GetFileName(adrFile)}: 发现 {h1Count} 个 # 标题，应该有且仅有 1 个");
            }
        }

        violations.Should().BeEmpty(
            $"以下 ADR 文件违反 ADR-946_1_1（标题级别即语义级别）：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-946_1_2: 关键语义块标题约束
    /// 验证关键语义块（Relationships、Decision、Enforcement、Glossary）必须使用 ## 级别且不能重复（§ADR-946_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-946_1_2: 关键语义块标题必须使用 ## 级别且不能重复")]
    public void ADR_946_1_2_Key_Semantic_Blocks_Must_Use_H2_And_Not_Duplicate()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrPath = Path.Combine(repoRoot, AdrDocsPath);

        if (!Directory.Exists(adrPath))
        {
            throw new DirectoryNotFoundException($"ADR 目录不存在: {adrPath}");
        }

        var adrFiles = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TIMELINE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("CHECKLIST", StringComparison.OrdinalIgnoreCase))
            .Where(f => Path.GetFileName(f).StartsWith("ADR-", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var lines = content.Split('\n');

            var inCodeBlock = false;
            var semanticBlockCounts = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                
                // 检测代码块
                if (trimmed.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                // 只在非代码块中检查语义块标题
                if (!inCodeBlock)
                {
                    foreach (var semanticHeading in KeySemanticHeadings)
                    {
                        // 检查是否是 ## 级别的关键语义块
                        // 使用严格匹配：## <语义块名称>（中文或英文，可能带括号）
                        var pattern = $@"^##\s+{Regex.Escape(semanticHeading)}(?:\s*（.*?）|\s*\(.*?\))?(?:\s|$)";
                        if (Regex.IsMatch(line, pattern))
                        {
                            if (!semanticBlockCounts.ContainsKey(semanticHeading))
                            {
                                semanticBlockCounts[semanticHeading] = 0;
                            }
                            semanticBlockCounts[semanticHeading]++;
                        }

                        // 检查是否在模板/示例中错误使用了 ## 级别（不带 Example 等后缀）
                        var wrongPattern = $@"^##\s+{Regex.Escape(semanticHeading)}\s*Example";
                        if (Regex.IsMatch(line, wrongPattern))
                        {
                            violations.Add($"{Path.GetFileName(adrFile)}: 在模板/示例中使用了 ## 级别的 '{semanticHeading} Example'，应使用 ### 或更低级别");
                        }
                    }
                }
            }

            // 检查重复的语义块
            foreach (var kvp in semanticBlockCounts)
            {
                if (kvp.Value > 1)
                {
                    violations.Add($"{Path.GetFileName(adrFile)}: 关键语义块 '{kvp.Key}' 出现了 {kvp.Value} 次，只能出现 1 次");
                }
            }
        }

        violations.Should().BeEmpty(
            $"以下 ADR 文件违反 ADR-946_1_2（关键语义块标题约束）：\n{string.Join("\n", violations)}");
    }

}
