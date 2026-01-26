using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text.RegularExpressions;
using Zss.BilliardHall.AdrSemanticParser.Models;

namespace Zss.BilliardHall.AdrSemanticParser;

/// <summary>
/// ADR 语义解析器，将 ADR Markdown 文档解析为结构化的语义模型
/// </summary>
public sealed class AdrParser
{
    private static readonly Regex AdrIdRegex = new(@"ADR-(\d+)", RegexOptions.Compiled);
    private static readonly Regex AdrLinkRegex = new(@"\[ADR-(\d+)[：:](.*?)\]", RegexOptions.Compiled);
    
    private readonly MarkdownPipeline _pipeline;

    public AdrParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    /// <summary>
    /// 解析 ADR 文档
    /// </summary>
    /// <param name="markdown">ADR 文档的 Markdown 内容</param>
    /// <param name="filePath">文档文件路径（可选）</param>
    /// <returns>解析后的 ADR 语义模型</returns>
    public AdrSemanticModel Parse(string markdown, string? filePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(markdown);

        var document = Markdown.Parse(markdown, _pipeline);
        
        var id = ExtractId(markdown, filePath);
        var title = ExtractTitle(document);
        var metadata = ExtractMetadata(document, markdown);
        var relationships = ExtractRelationships(document, markdown);
        var glossary = ExtractGlossary(document, markdown);
        var quickReference = ExtractQuickReference(document, markdown);

        return new AdrSemanticModel
        {
            Id = id,
            Title = title,
            Status = metadata.Status,
            Level = metadata.Level,
            Scope = metadata.Scope,
            Version = metadata.Version,
            EffectiveDate = metadata.EffectiveDate,
            Relationships = relationships,
            FilePath = filePath,
            DecisionContent = ExtractDecisionContent(document, markdown),
            Glossary = glossary,
            QuickReference = quickReference
        };
    }

    /// <summary>
    /// 从文件解析 ADR 文档
    /// </summary>
    public async Task<AdrSemanticModel> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Parse(markdown, filePath);
    }

    private static string ExtractId(string markdown, string? filePath)
    {
        // 尝试从文件名提取
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = AdrIdRegex.Match(fileName);
            if (match.Success)
            {
                return $"ADR-{match.Groups[1].Value}";
            }
        }

        // 尝试从标题提取
        var firstLine = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        var idMatch = AdrIdRegex.Match(firstLine);
        if (idMatch.Success)
        {
            return $"ADR-{idMatch.Groups[1].Value}";
        }

        throw new InvalidOperationException("无法从文档中提取 ADR 编号");
    }

    private static string ExtractTitle(MarkdownDocument document)
    {
        var heading = document.Descendants<HeadingBlock>().FirstOrDefault();
        if (heading == null)
        {
            throw new InvalidOperationException("文档缺少标题");
        }

        var title = heading.Inline?.FirstChild?.ToString() ?? "";
        
        // 移除 ADR 编号前缀
        var colonIndex = title.IndexOf('：');
        if (colonIndex == -1) colonIndex = title.IndexOf(':');
        
        if (colonIndex > 0)
        {
            title = title[(colonIndex + 1)..].Trim();
        }

        return title;
    }

    private static (string? Status, string? Level, string? Scope, string? Version, string? EffectiveDate) 
        ExtractMetadata(MarkdownDocument document, string markdown)
    {
        string? status = null, level = null, scope = null, version = null, effectiveDate = null;

        // 查找元数据部分（通常在文档开头）
        var lines = markdown.Split('\n');
        foreach (var line in lines.Take(20)) // 前20行
        {
            var trimmed = line.Trim();
            
            if (trimmed.StartsWith("**状态**", StringComparison.OrdinalIgnoreCase))
            {
                status = ExtractMetadataValue(trimmed);
            }
            else if (trimmed.StartsWith("**级别**", StringComparison.OrdinalIgnoreCase))
            {
                level = ExtractMetadataValue(trimmed);
            }
            else if (trimmed.StartsWith("**适用范围**", StringComparison.OrdinalIgnoreCase))
            {
                scope = ExtractMetadataValue(trimmed);
            }
            else if (trimmed.StartsWith("**版本**", StringComparison.OrdinalIgnoreCase))
            {
                version = ExtractMetadataValue(trimmed);
            }
            else if (trimmed.StartsWith("**生效时间**", StringComparison.OrdinalIgnoreCase))
            {
                effectiveDate = ExtractMetadataValue(trimmed);
            }
        }

        return (status, level, scope, version, effectiveDate);
    }

    private static string? ExtractMetadataValue(string line)
    {
        var parts = line.Split('：', ':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return parts[1].Trim().TrimEnd();
        }
        return null;
    }

    private static AdrRelationships ExtractRelationships(MarkdownDocument document, string markdown)
    {
        var relationships = new AdrRelationships();

        // 查找关系声明章节
        var relationshipSection = FindSection(markdown, "关系声明", "Relationships");
        if (string.IsNullOrWhiteSpace(relationshipSection))
        {
            return relationships;
        }

        // 提取各类关系
        relationships.DependsOn.AddRange(ExtractReferences(relationshipSection, "依赖", "Depends On"));
        relationships.DependedBy.AddRange(ExtractReferences(relationshipSection, "被依赖", "Depended By"));
        relationships.Supersedes.AddRange(ExtractReferences(relationshipSection, "替代", "Supersedes"));
        relationships.SupersededBy.AddRange(ExtractReferences(relationshipSection, "被替代", "Superseded By"));
        relationships.Related.AddRange(ExtractReferences(relationshipSection, "相关", "Related"));

        return relationships;
    }

    private static List<AdrReference> ExtractReferences(string section, params string[] keywords)
    {
        var references = new List<AdrReference>();
        
        foreach (var keyword in keywords)
        {
            var pattern = $@"\*\*{keyword}[（(].*?[)）]\*\*[:：]";
            var regex = new Regex(pattern, RegexOptions.Multiline);
            var match = regex.Match(section);
            
            if (!match.Success) continue;

            var startIndex = match.Index + match.Length;
            var nextHeaderIndex = section.IndexOf("**", startIndex);
            var subsection = nextHeaderIndex > 0 
                ? section[startIndex..nextHeaderIndex] 
                : section[startIndex..];

            // 提取 ADR 引用
            var lines = subsection.Split('\n');
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("- [ADR-") || line.Trim().StartsWith("- 无"))
                {
                    var reference = ParseAdrReference(line);
                    if (reference != null)
                    {
                        references.Add(reference);
                    }
                }
            }
        }

        return references;
    }

    private static AdrReference? ParseAdrReference(string line)
    {
        // 匹配格式：- [ADR-XXXX：标题](路径) - 原因说明
        var match = Regex.Match(line, @"\[ADR-(\d+)[：:](.*?)\]\((.*?)\)(?:\s*-\s*(.*))?");
        if (!match.Success)
        {
            return null;
        }

        var id = $"ADR-{match.Groups[1].Value}";
        var title = match.Groups[2].Value.Trim();
        var path = match.Groups[3].Value.Trim();
        var reason = match.Groups.Count > 4 ? match.Groups[4].Value.Trim() : null;

        return new AdrReference
        {
            Id = id,
            Title = title,
            RelativePath = path,
            Reason = reason
        };
    }

    private static string? ExtractDecisionContent(MarkdownDocument document, string markdown)
    {
        return FindSection(markdown, "决策", "Decision");
    }

    private static List<GlossaryEntry> ExtractGlossary(MarkdownDocument document, string markdown)
    {
        var entries = new List<GlossaryEntry>();
        var glossarySection = FindSection(markdown, "术语表", "Glossary");
        
        if (string.IsNullOrWhiteSpace(glossarySection))
        {
            return entries;
        }

        // 解析表格格式的术语表
        var lines = glossarySection.Split('\n');
        var inTable = false;
        
        foreach (var line in lines)
        {
            if (line.Contains('|') && !line.TrimStart().StartsWith("|---"))
            {
                if (!inTable && (line.Contains("术语") || line.Contains("Term")))
                {
                    inTable = true;
                    continue;
                }
                
                if (inTable)
                {
                    var parts = line.Split('|', StringSplitOptions.TrimEntries)
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToArray();
                    
                    if (parts.Length >= 2)
                    {
                        entries.Add(new GlossaryEntry
                        {
                            Term = parts[0],
                            Definition = parts[1],
                            EnglishTerm = parts.Length > 2 ? parts[2] : null
                        });
                    }
                }
            }
        }

        return entries;
    }

    private static List<QuickReferenceEntry> ExtractQuickReference(MarkdownDocument document, string markdown)
    {
        var entries = new List<QuickReferenceEntry>();
        var quickRefSection = FindSection(markdown, "快速参考表", "Quick Reference");
        
        if (string.IsNullOrWhiteSpace(quickRefSection))
        {
            return entries;
        }

        // 解析表格格式的快速参考
        var lines = quickRefSection.Split('\n');
        var inTable = false;
        
        foreach (var line in lines)
        {
            if (line.Contains('|') && !line.TrimStart().StartsWith("|---"))
            {
                if (!inTable && (line.Contains("约束") || line.Contains("Constraint")))
                {
                    inTable = true;
                    continue;
                }
                
                if (inTable)
                {
                    var parts = line.Split('|', StringSplitOptions.TrimEntries)
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToArray();
                    
                    if (parts.Length >= 2)
                    {
                        entries.Add(new QuickReferenceEntry
                        {
                            ConstraintId = parts[0],
                            Description = parts[1],
                            TestMethod = parts.Length > 2 ? parts[2] : null,
                            TestCase = parts.Length > 3 ? parts[3] : null,
                            IsMandatory = parts.Length > 4 && parts[4].Contains("✅")
                        });
                    }
                }
            }
        }

        return entries;
    }

    private static string? FindSection(string markdown, params string[] headings)
    {
        foreach (var heading in headings)
        {
            var pattern = $@"##\s+{Regex.Escape(heading)}.*?\n(.*?)(?=\n##|\z)";
            var match = Regex.Match(markdown, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }
}
