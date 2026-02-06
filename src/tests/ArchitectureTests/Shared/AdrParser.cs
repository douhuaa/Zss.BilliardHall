namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR 文档解析器
/// 使用 Markdig 进行 AST 级别的 Markdown 解析
/// </summary>
public static class AdrParser
{
    private static readonly Regex AdrIdPattern = new(@"ADR-\d{3,4}", RegexOptions.Compiled);
    private static readonly Regex FrontMatterPattern = new(@"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n", 
        RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// 解析 ADR 文档，提取关系声明和 Front Matter
    /// </summary>
    public static AdrDocument Parse(string adrId, string filePath)
    {
        var text = File.ReadAllText(filePath);
        
        // 先解析 Front Matter
        var (hasFrontMatter, adrField, typeField, statusField, levelField) = ParseFrontMatter(text);
        
        // 判断是否是正式 ADR
        var isAdr = DetermineIsAdr(adrField, typeField, filePath, hasFrontMatter);

        // 创建 ADR 文档对象
        var adr = new AdrDocument
        {
            Id = adrId,
            FilePath = filePath,
            HasFrontMatter = hasFrontMatter,
            AdrField = adrField,
            Type = typeField,
            Status = statusField,
            Level = levelField,
            IsAdr = isAdr
        };

        // 解析关系声明
        var pipeline = new MarkdownPipelineBuilder().Build();
        var document = Markdown.Parse(text, pipeline);
        ParseRelationships(document, adr);
        
        return adr;
    }

    /// <summary>
    /// 解析 YAML Front Matter
    /// 提取关键字段：adr, type, status, level
    /// </summary>
    private static (bool hasFrontMatter, string? adrField, string? typeField, string? statusField, string? levelField) 
        ParseFrontMatter(string text)
    {
        var match = FrontMatterPattern.Match(text);
        if (!match.Success)
        {
            return (false, null, null, null, null);
        }

        var frontMatterText = match.Groups[1].Value;
        
        // 简单的键值对解析（不依赖 YamlDotNet）
        var lines = frontMatterText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string? adrField = null;
        string? typeField = null;
        string? statusField = null;
        string? levelField = null;

        foreach (var line in lines)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;

            var key = line.Substring(0, colonIndex).Trim();
            var value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');

            switch (key.ToLowerInvariant())
            {
                case "adr":
                    adrField = value;
                    break;
                case "type":
                    typeField = value;
                    break;
                case "status":
                    statusField = value;
                    break;
                case "level":
                    levelField = value;
                    break;
            }
        }

        return (true, adrField, typeField, statusField, levelField);
    }

    /// <summary>
    /// 判断是否是正式的 ADR 文档
    /// 规则：
    /// 1. Front Matter 中有 adr 字段（如 "ADR-001"）
    /// 2. type 字段不是 "checklist", "guide", "template"
    /// 3. 文件名不包含排除关键字
    /// </summary>
    private static bool DetermineIsAdr(string? adrField, string? typeField, string filePath, bool hasFrontMatter)
    {
        var fileName = Path.GetFileName(filePath);

        // 排除明确标记为非 ADR 的类型
        if (!string.IsNullOrEmpty(typeField))
        {
            var lowerType = typeField.ToLowerInvariant();
            if (lowerType == "checklist" || lowerType == "guide" || 
                lowerType == "template" || lowerType == "proposal")
            {
                return false;
            }
        }

        // 排除文件名包含特定关键字的
        if (fileName.Contains("README", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 特殊处理 checklist：如果有 adr 字段，仍然算 ADR
        if (fileName.Contains("checklist", StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrEmpty(adrField);
        }

        // 如果没有 Front Matter，根据文件名判断
        if (!hasFrontMatter)
        {
            return !fileName.Contains("guide", StringComparison.OrdinalIgnoreCase) &&
                   !fileName.Contains("proposal", StringComparison.OrdinalIgnoreCase);
        }

        // 如果有 adr 字段且不为空，认为是正式 ADR
        if (!string.IsNullOrEmpty(adrField))
        {
            return true;
        }

        // 默认：有 Front Matter 且 type 为 adr 或未指定，认为是 ADR
        return typeField == null || typeField.Equals("adr", StringComparison.OrdinalIgnoreCase);
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
                }
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

        return null;
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
}
