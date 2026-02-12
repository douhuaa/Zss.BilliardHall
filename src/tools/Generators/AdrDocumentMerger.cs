using System.Text;
using Markdig;
using Markdig.Syntax;
using Zss.BilliardHall.Specification.Rules;

namespace Zss.BilliardHall.Generators;

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
        _pipeline = new MarkdownPipelineBuilder().Build();
    }

    /// <summary>
    /// 合并生成的 Decision 章节与现有 ADR 文档
    /// </summary>
    public string MergeDecisionSection(string existingAdrContent, ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(existingAdrContent);
        ArgumentNullException.ThrowIfNull(ruleSet);

        var newDecisionContent = _decisionGenerator.GenerateDecisionSection(ruleSet, options ?? DecisionGenerationOptions.Default);
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

        // 提取 Front Matter（如果存在）
        var frontMatter = ExtractRawFrontMatter(existingAdrContent);

        // 提取所有章节
        var sections = ExtractSections(document, existingAdrContent);

        // 构建新文档（使用显式 \n 而非 AppendLine 以确保跨平台一致性）
        var result = new StringBuilder();

        // 1. 添加 Front Matter
        if (!string.IsNullOrEmpty(frontMatter))
        {
            result.Append(frontMatter);
            result.Append("\n\n");
        }

        // 2. 定义章节顺序
        var sectionOrder = new[]
        {
            "Focus", "Glossary", "Decision", "Context", "Consequences", "References"
        };

        // 3. 按顺序添加章节
        foreach (var sectionName in sectionOrder)
        {
            if (sectionName == "Decision")
            {
                // 使用新生成的 Decision 章节
                result.Append(newDecisionContent.TrimEnd());
                result.Append("\n\n");
            }
            else if (sections.ContainsKey(sectionName))
            {
                // 保留现有章节
                result.Append(sections[sectionName].TrimEnd());
                result.Append("\n\n");
            }
        }

        // 4. 添加其他未在标准顺序中的章节
        foreach (var (name, content) in sections)
        {
            if (!sectionOrder.Contains(name))
            {
                result.Append(content.TrimEnd());
                result.Append("\n\n");
            }
        }

        // 移除末尾多余的空行
        return result.ToString().TrimEnd() + "\n";
    }

    /// <summary>
    /// 提取原始 Front Matter 文本（包括 --- 分隔符）
    /// </summary>
    private static string? ExtractRawFrontMatter(string content)
    {
        if (!content.StartsWith("---"))
            return null;

        var lines = content.Split('\n');
        var endIndex = -1;

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex == -1)
            return null;

        return string.Join('\n', lines.Take(endIndex + 1));
    }

    /// <summary>
    /// 提取文档中的所有章节
    /// </summary>
    private static Dictionary<string, string> ExtractSections(MarkdownDocument document, string originalContent)
    {
        var sections = new Dictionary<string, string>();
        var lines = originalContent.Split('\n');

        // 跳过 Front Matter
        int startLine = 0;
        if (originalContent.StartsWith("---"))
        {
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    startLine = i + 1;
                    break;
                }
            }
        }

        // 查找所有 H2 标题
        var headings = document.Descendants<HeadingBlock>()
            .Where(h => h.Level == 2)
            .OrderBy(h => h.Line)
            .ToList();

        for (int i = 0; i < headings.Count; i++)
        {
            var heading = headings[i];
            var sectionName = ExtractSectionName(lines[heading.Line]);

            if (string.IsNullOrWhiteSpace(sectionName))
                continue;

            // 确定章节内容的起始和结束行
            int sectionStart = heading.Line;
            int sectionEnd = (i < headings.Count - 1) ? headings[i + 1].Line - 1 : lines.Length - 1;

            // 提取章节内容
            var sectionLines = new List<string>();
            for (int lineNum = sectionStart; lineNum <= sectionEnd; lineNum++)
            {
                if (lineNum < lines.Length)
                {
                    sectionLines.Add(lines[lineNum]);
                }
            }

            var sectionContent = string.Join('\n', sectionLines).TrimEnd();
            sections[sectionName] = sectionContent;
        }

        return sections;
    }

    /// <summary>
    /// 从标题行提取章节名称（去除中英文标题）
    /// 例如：## Decision（裁决） -> Decision
    /// </summary>
    private static string ExtractSectionName(string headingLine)
    {
        if (string.IsNullOrWhiteSpace(headingLine))
            return string.Empty;

        // 移除 ## 标记
        var text = headingLine.TrimStart('#').Trim();

        // 提取第一个词（假设是英文章节名）
        var parts = text.Split(new[] { ' ', '（', '(', '：', ':' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : string.Empty;
    }
}
