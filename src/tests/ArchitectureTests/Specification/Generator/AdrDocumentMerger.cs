namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// ADR 文档合并器实现
/// 使用 Markdig 解析 Markdown 文档，保留除 Decision 外的所有章节
/// </summary>
public sealed class AdrDocumentMerger : IAdrDocumentMerger
{
    private readonly IAdrDecisionGenerator _decisionGenerator;
    private readonly MarkdownPipeline _pipeline;

    public AdrDocumentMerger(IAdrDecisionGenerator decisionGenerator)
    {
        _decisionGenerator = decisionGenerator ?? throw new ArgumentNullException(nameof(decisionGenerator));
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    /// <summary>
    /// 合并生成的 Decision 章节与现有 ADR 文档
    /// </summary>
    public string MergeDecisionSection(string existingAdrContent, ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(existingAdrContent);
        ArgumentNullException.ThrowIfNull(ruleSet);

        var newDecisionContent = _decisionGenerator.GenerateDecisionSection(
        ruleSet, 
        options ?? DecisionGenerationOptions.Default);

        return MergeDecisionSection(existingAdrContent, newDecisionContent);
    }

    /// <summary>
    /// 合并生成的 Decision 章节与现有 ADR 文档
    /// </summary>
    public string MergeDecisionSection(string existingAdrContent, string newDecisionContent)
    {
        ArgumentNullException.ThrowIfNull(existingAdrContent);
        ArgumentNullException.ThrowIfNull(newDecisionContent);

        // 解析现有 ADR 文档
        var document = Markdown.Parse(existingAdrContent, _pipeline);

        // 提取 Front Matter（如果存在）- 使用 Shared 中的统一方法
        var frontMatter = FrontMatterParser.ExtractRawFrontMatter(existingAdrContent);

        // 提取所有章节
        var sections = ExtractSections(document, existingAdrContent);

        // 构建新文档（使用显式 \n 而非 AppendLine 以确保跨平台一致性）
        var result = new System.Text.StringBuilder();

        // 1. 添加 Front Matter
        if (!string.IsNullOrEmpty(frontMatter))
        {
            result.Append(frontMatter);
            result.Append("\n\n");
        }

        // 2. 添加 Decision 之前的章节（如 Focus、Glossary）
        foreach (var section in sections.Where(s => s.Order < GetDecisionOrder()))
        {
            result.Append(section.Content);
            result.Append("\n\n");
        }

        // 3. 添加新的 Decision 章节
        result.Append(newDecisionContent);
        if (!newDecisionContent.EndsWith("\n\n"))
        {
            result.Append("\n");
        }

        // 4. 添加 Decision 之后的章节（如 Context、Consequences）
        foreach (var section in sections.Where(s => s.Order > GetDecisionOrder()))
        {
            result.Append(section.Content);
            result.Append("\n\n");
        }

        // 统一行尾为 LF，避免跨平台差异
        return NormalizeNewlines(result.ToString().TrimEnd() + "\n");
    }

    /// <summary>
    /// 提取文档中的所有章节
    /// </summary>
    private static List<DocumentSection> ExtractSections(MarkdownDocument document, string content)
    {
        var sections = new List<DocumentSection>();
        var lines = content.Split('\n');

        // 提取所有 H2 级别的章节
        var headings = document.Descendants<HeadingBlock>()
            .Where(h => h.Level == 2)
            .ToList();

        foreach (var heading in headings)
        {
            var title = GetHeadingText(heading);
            
            // 跳过 Decision 章节（将被替换）
            if (title.Contains("Decision") || title.Contains("裁决"))
                continue;

            var sectionContent = ExtractSectionContent(heading, lines, headings);
            var order = GetSectionOrder(title);

            sections.Add(new DocumentSection
            {
                Title = title,
                Content = sectionContent,
                Order = order
            });
        }

        return sections;
    }

    /// <summary>
    /// 获取标题文本
    /// </summary>
    private static string GetHeadingText(HeadingBlock heading)
    {
        var inline = heading.Inline;
        if (inline == null) return string.Empty;

        var text = new System.Text.StringBuilder();
        foreach (var literal in inline.OfType<Markdig.Syntax.Inlines.LiteralInline>())
        {
            text.Append(literal.Content);
        }
        
        return text.ToString();
    }

    /// <summary>
    /// 提取章节内容
    /// </summary>
    private static string ExtractSectionContent(HeadingBlock heading, string[] lines, List<HeadingBlock> allHeadings)
    {
        var startLine = heading.Line;
        var endLine = lines.Length;

        // 找到下一个同级或更高级别的标题
        var nextHeading = allHeadings
            .Where(h => h.Line > heading.Line && h.Level <= heading.Level)
            .OrderBy(h => h.Line)
            .FirstOrDefault();

        if (nextHeading != null)
        {
            endLine = nextHeading.Line;
        }

        var sectionLines = lines.Skip(startLine).Take(endLine - startLine);
        return string.Join('\n', sectionLines).TrimEnd();
    }

    /// <summary>
    /// 获取章节的排序顺序
    /// </summary>
    private static int GetSectionOrder(string title)
    {
        var normalizedTitle = title.ToLowerInvariant();

        if (normalizedTitle.Contains("focus") || normalizedTitle.Contains("聚焦"))
            return 1;
        if (normalizedTitle.Contains("glossary") || normalizedTitle.Contains("术语"))
            return 2;
        if (normalizedTitle.Contains("decision") || normalizedTitle.Contains("裁决"))
            return 3;
        if (normalizedTitle.Contains("context") || normalizedTitle.Contains("上下文"))
            return 4;
        if (normalizedTitle.Contains("consequence") || normalizedTitle.Contains("影响"))
            return 5;
        if (normalizedTitle.Contains("reference") || normalizedTitle.Contains("参考"))
            return 6;

        return 99; // 其他章节放在最后
    }

    /// <summary>
    /// 获取 Decision 章节的排序顺序
    /// </summary>
    private static int GetDecisionOrder() => 3;

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");

    /// <summary>
    /// 文档章节
    /// </summary>
    private sealed class DocumentSection
    {
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public int Order { get; init; }
    }
}