using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR 文档解析器
/// 使用 Markdig 进行 AST 级别的 Markdown 解析
/// </summary>
public static class AdrParser
{
    private static readonly Regex AdrIdPattern = new(@"ADR-\d{3,4}", RegexOptions.Compiled);

    /// <summary>
    /// 解析 ADR 文档，提取关系声明
    /// </summary>
    public static AdrDocument Parse(string adrId, string filePath)
    {
        var text = File.ReadAllText(filePath);
        var pipeline = new MarkdownPipelineBuilder().Build();
        var document = Markdown.Parse(text, pipeline);

        var adr = new AdrDocument
        {
            Id = adrId,
            FilePath = filePath
        };

        ParseRelationships(document, adr);
        return adr;
    }

    /// <summary>
    /// 从 Markdown AST 中提取关系声明
    /// 支持中英文双语格式：
    /// - **依赖（Depends On）**：
    /// - **Depends On**：
    /// </summary>
    private static void ParseRelationships(MarkdownDocument doc, AdrDocument adr)
    {
        string? currentSection = null;
        bool inRelationshipsSection = false;

        foreach (var node in doc.Descendants())
        {
            // 检测 ## Relationships（关系声明）或 ## 关系声明
            if (node is HeadingBlock heading && heading.Level == 2)
            {
                var headingText = ExtractText(heading);
                if (headingText.Contains("Relationships", StringComparison.OrdinalIgnoreCase) ||
                    headingText.Contains("关系声明", StringComparison.OrdinalIgnoreCase))
                {
                    inRelationshipsSection = true;
                else if (inRelationshipsSection)
                {
                    // 遇到下一个 ## 标题，退出关系声明区
                    break;
                }
                continue;
            }

            // 只在关系声明区内处理
            if (!inRelationshipsSection)
                continue;

            // 检测关系类型的粗体标题
            if (node is ParagraphBlock paragraph)
            {
                var text = ExtractText(paragraph);
                currentSection = DetermineRelationType(text);
                continue;
            }

            // 提取列表项中的 ADR 引用
            if (node is ListItemBlock listItem && currentSection is not null)
            {
                var itemText = ExtractText(listItem);
                var matches = AdrIdPattern.Matches(itemText);
                
                foreach (Match match in matches)
                {
                    AddRelation(adr, currentSection, match.Value);
                }
            }
        }
    }

    /// <summary>
    /// 从 Markdown 块中提取纯文本
    /// </summary>
    private static string ExtractText(Block block)
    {
        var text = new System.Text.StringBuilder();
        
        foreach (var inline in block.Descendants<Inline>())
        {
            if (inline is LiteralInline literal)
            {
                text.Append(literal.Content);
            }
        }
        
        return text.ToString();
    }

    /// <summary>
    /// 判断当前段落是哪种关系类型
    /// 支持中英文双语：
    /// - **依赖（Depends On）**：
    /// - **Depends On**：
    /// </summary>
    private static string? DetermineRelationType(string text)
    {
        // 规范化文本，去除空白和标点
        var normalized = text.Trim().TrimEnd('：', ':');

        if (normalized.Contains("Depends On", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("依赖", StringComparison.OrdinalIgnoreCase))
        {
            return "DependsOn";
        }
        
        if (normalized.Contains("Depended By", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("被依赖", StringComparison.OrdinalIgnoreCase))
        {
            return "DependedBy";
        }
        
        if (normalized.Contains("Supersedes", StringComparison.OrdinalIgnoreCase) &&
            !normalized.Contains("Superseded By", StringComparison.OrdinalIgnoreCase) &&
            !normalized.Contains("被替代", StringComparison.OrdinalIgnoreCase))
        {
            return "Supersedes";
        }
        
        if (normalized.Contains("Superseded By", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("被替代", StringComparison.OrdinalIgnoreCase))
        {
            return "SupersededBy";
        }
        
        if (normalized.Contains("Related", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("相关", StringComparison.OrdinalIgnoreCase))
        {
            return "Related";
        }

    /// <summary>
    /// 将提取的关系添加到 ADR 模型中
    /// </summary>
    private static void AddRelation(AdrDocument adr, string relationType, string targetAdr)
    {
        switch (relationType)
        {
            case "DependsOn":
                adr.DependsOn.Add(targetAdr);
                break;
            case "DependedBy":
                adr.DependedBy.Add(targetAdr);
                break;
            case "Supersedes":
                adr.Supersedes.Add(targetAdr);
                break;
            case "SupersededBy":
                adr.SupersededBy.Add(targetAdr);
                break;
            case "Related":
                adr.Related.Add(targetAdr);
                break;
}
}
