using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Enforcement;

/// <summary>
/// 文档裁决语言检查 - Enforcement 层测试
/// 
/// 【定位】：执行 ADR-008 的具体约束
/// 【来源】：ADR-008 决策 2.2 和决策 3.3
/// 【执法】：失败 = CI 阻断
/// 
/// 本测试类检查：
/// 1. README/Guide 不得使用裁决性语言（必须、禁止等）
/// 2. 允许在特定上下文中使用（引用 ADR 时）
/// 
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
/// - 来源决策: ADR-008 决策 2.2、3.3
/// </summary>
public sealed class DocumentationDecisionLanguageTests
{
    [Fact(DisplayName = "README/Guide 不得使用裁决性语言")]
    public void README_Must_Not_Use_Decision_Language()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        
        // 裁决性词汇（ADR-008 明确禁止 README 使用）
        var forbiddenWords = new[] { "必须", "禁止", "不允许", "不得", "应当" };
        
        // 例外：可以在引用 ADR 的上下文中使用，或在示例标记中使用
        var allowedContextPatterns = new[]
        {
            @"根据\s*ADR[-\s]*\d+",  // "根据 ADR-001"
            @"参考\s*ADR[-\s]*\d+",  // "参考 ADR-001"
            @"详见\s*ADR[-\s]*\d+",  // "详见 ADR-001"
            @"ADR[-\s]*\d+\s*规定",  // "ADR-001 规定"
            @"本文档无裁决力",        // 声明部分
            @"[❌✅]\s*(禁止|必须)",  // ❌ 禁止（示例标记）
            @"\|\s*(禁止|必须)",     // 表格中的内容
            @"^[\s*#]*\*\*[❌✅]",   // **❌ 禁止：** 标题
            @"^[\s-]*[❌✅]"         // 列表项标记
        };

        var violations = new List<string>();
        
        // 扫描 docs 目录下的 README 和 Guide
        var docsDir = Path.Combine(repoRoot, "docs");
        if (!Directory.Exists(docsDir)) return;

        var readmeFiles = Directory.GetFiles(docsDir, "*.md", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase) 
                     || f.Contains("guide", StringComparison.OrdinalIgnoreCase)
                     || f.Contains("GUIDE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("/adr/", StringComparison.OrdinalIgnoreCase)) // 排除 ADR 文档
            .Where(f => !f.Contains("/summaries/", StringComparison.OrdinalIgnoreCase)) // 排除总结文档
            .Where(f => !f.Contains("/templates/", StringComparison.OrdinalIgnoreCase)) // 排除模板
            .Take(20); // 限制检查数量以提高性能

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 移除代码块和引用块
            var contentWithoutCodeBlocks = RemoveCodeBlocks(content);
            var contentWithoutQuotes = RemoveQuotedSections(contentWithoutCodeBlocks);
            
            var lines = contentWithoutQuotes.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                foreach (var word in forbiddenWords)
                {
                    if (line.Contains(word))
                    {
                        // 检查是否在允许的上下文中
                        var isAllowedContext = allowedContextPatterns.Any(pattern => 
                            Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
                        
                        if (!isAllowedContext)
                        {
                            violations.Add($"  • {relativePath}:{i + 1} - 使用裁决词 '{word}'");
                            violations.Add($"    内容: {line.Trim().Substring(0, Math.Min(80, line.Trim().Length))}");
                        }
                    }
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ Enforcement 违规：以下 README/Guide 使用了裁决性语言",
                "",
                "根据 ADR-008 决策 2.2：README/Guide 只能解释'如何使用'，不得使用裁决性语言。",
                ""
            .Concat(violations.Take(10)) // 限制输出前10个违规
            .Concat(violations.Count > 10 ? new[] { $"  ... 还有 {violations.Count - 10} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 将裁决性语句改为引用 ADR：'根据 ADR-XXX，模块使用事件通信'",
                "  2. 在文档开头添加：'> ⚠️ 本文档无裁决力，所有架构决策以 ADR 正文为准'",
                "  3. 使用描述性语言代替命令性语言",
                "",
                "参考：docs/adr/constitutional/ADR-008-documentation-governance-constitution.md 决策 3.3"
            })));
        }
    }

    private static string RemoveCodeBlocks(string content)
    {
        // 移除 Markdown 代码块
        return Regex.Replace(content, @"```[\s\S]*?```", string.Empty);
    }

    private static string RemoveQuotedSections(string content)
    {
        // 移除引用块（> 开头的行）
        var lines = content.Split('\n');
        var nonQuotedLines = lines.Where(line => !line.TrimStart().StartsWith(">"));
        return string.Join("\n", nonQuotedLines);
    }
}
}
