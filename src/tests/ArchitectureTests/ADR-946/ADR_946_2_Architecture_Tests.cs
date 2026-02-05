using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_946;

/// <summary>
/// ADR-946_2: 模板与示例结构约束
/// 验证 ADR 文档中模板和示例避免解析歧义
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-946_2_1: 模板与示例必须避免解析歧义
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md
/// </summary>
public sealed class ADR_946_2_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    
    // 关键语义块标题（在示例/模板中不能直接使用 ## 级别）
    private static readonly string[] KeySemanticHeadings = new[]
    {
        "Relationships",
        "Decision",
        "Enforcement",
        "Glossary",
        "Non-Goals",
        "Prohibited",
        "References",
        "History"
    };

    /// <summary>
    /// ADR-946_2_1: 模板与示例必须避免解析歧义
    /// 验证示例/模板使用英文、占位符或降级标题，不使用语义 ## 标题（§ADR-946_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-946_2_1: 模板与示例必须避免解析歧义")]
    public void ADR_946_2_1_Templates_And_Examples_Must_Avoid_Parsing_Ambiguity()
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
            var lineNumber = 0;

            foreach (var line in lines)
            {
                lineNumber++;
                var trimmed = line.TrimStart();
                
                // 检测代码块边界
                if (trimmed.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                // 在代码块外检查可能产生歧义的标题
                if (!inCodeBlock)
                {
                    foreach (var semanticHeading in KeySemanticHeadings)
                    {
                        // 检查是否在示例上下文中使用了 ## 级别的语义标题
                        // 允许：## Relationships Example, ## Decision Example, ### Relationships
                        // 不允许：在 Example/Template/示例 章节中直接使用 ## Relationships

                        // 检测可能的违规模式：
                        // 1. 代码块外的 ## <语义块> Template
                        // 2. 代码块外的 ## <语义块> 示例
                        var templatePattern = $@"^##\s+{Regex.Escape(semanticHeading)}\s+(Template|模板)";
                        if (Regex.IsMatch(line, templatePattern, RegexOptions.IgnoreCase))
                        {
                            violations.Add($"{Path.GetFileName(adrFile)}:{lineNumber} - 在模板中使用了 ## 级别的 '{semanticHeading}'，应使用 ### 或代码块");
                        }

                        var examplePattern = $@"^##\s+{Regex.Escape(semanticHeading)}\s+(Example|示例)";
                        if (Regex.IsMatch(line, examplePattern, RegexOptions.IgnoreCase))
                        {
                            violations.Add($"{Path.GetFileName(adrFile)}:{lineNumber} - 在示例中使用了 ## 级别的 '{semanticHeading}'，应使用 ### 或代码块");
                        }
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            $"以下 ADR 文件违反 ADR-946_2_1（模板与示例结构约束）：\n{string.Join("\n", violations)}");
    }

}
