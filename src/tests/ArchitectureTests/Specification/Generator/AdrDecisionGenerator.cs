namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// ADR Decision 生成器实现
/// 将 ArchitectureRuleSet 转换为 Markdown 格式的 Decision 章节
/// </summary>
public sealed class AdrDecisionGenerator : IAdrDecisionGenerator
{
    private static readonly HashSet<char> MarkdownSpecialChars = new()
    {
        '\\', '`', '*', '_', '[', ']', '<', '>', '#'
    };

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节（使用默认选项）
    /// </summary>
    public string GenerateDecisionSection(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        return GenerateDecisionSection(ruleSet, DecisionGenerationOptions.Default);
    }

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节（带选项）
    /// </summary>
    public string GenerateDecisionSection(ArchitectureRuleSet ruleSet, DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        var sb = new StringBuilder();

        if (options.IncludeSectionHeader)
        {
            AppendSectionHeader(sb, options);
        }

        var orderedRules = ruleSet.Rules
            .OrderBy(r => r.Id.RuleNumber)
            .ToList();

        if (!orderedRules.Any())
            return NormalizeNewlines(sb.ToString());

        for (int i = 0; i < orderedRules.Count; i++)
        {
            AppendRuleSection(sb, orderedRules[i], ruleSet, options);

            if (i < orderedRules.Count - 1)
            {
                sb.AppendLine();
            }
        }

        // 统一行尾为 LF，避免跨平台差异
        return NormalizeNewlines(sb.ToString());
    }

    private static void AppendSectionHeader(StringBuilder sb, DecisionGenerationOptions options)
    {
        var headerPrefix = MakeHeaderPrefix(2 + options.HeaderLevelOffset);
        sb.AppendLine($"{headerPrefix} Decision（裁决）");
        sb.AppendLine();

        if (options.IncludeWarningNote)
        {
            sb.AppendLine("> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**");
            sb.AppendLine();
        }
    }

    private static void AppendRuleSection(
        StringBuilder sb,
        ArchitectureRuleDefinition rule,
        ArchitectureRuleSet ruleSet,
        DecisionGenerationOptions options)
    {
        if (rule is null)
            return;

        var summary = options.EscapeMarkdown ? EscapeMarkdown(rule.Summary) : rule.Summary;

        AppendRuleHeader(sb, rule.Id.ToString(), summary, options);

        var clauses = GetClausesForRule(rule, ruleSet);
        AppendRuleClauses(sb, clauses, options);
    }

    private static void AppendRuleHeader(StringBuilder sb, string ruleIdText, string summary, DecisionGenerationOptions options)
    {
        var headerPrefix = MakeHeaderPrefix(3 + options.HeaderLevelOffset);
        sb.AppendLine($"{headerPrefix} {ruleIdText}：{summary}（Rule）");
        sb.AppendLine();
    }

    private static List<ArchitectureClauseDefinition> GetClausesForRule(ArchitectureRuleDefinition rule, ArchitectureRuleSet ruleSet)
    {
        if (rule is null || ruleSet is null)
            return new List<ArchitectureClauseDefinition>();

        return ruleSet.Clauses
            .Where(c => c.Id.AdrNumber == rule.Id.AdrNumber && c.Id.RuleNumber == rule.Id.RuleNumber)
            .OrderBy(c => c.Id.ClauseNumber)
            .ToList();
    }

    private static void AppendRuleClauses(StringBuilder sb, IEnumerable<ArchitectureClauseDefinition> clauses, DecisionGenerationOptions options)
    {
        if (clauses == null)
            return;

        foreach (var clause in clauses)
        {
            AppendClauseSection(sb, clause, options);
        }
    }

    private static void AppendClauseSection(
        StringBuilder sb,
        ArchitectureClauseDefinition clause,
        DecisionGenerationOptions options)
    {
        if (clause is null)
            return;

        var condition = options.EscapeMarkdown ? EscapeMarkdown(clause.Condition) : clause.Condition;
        var enforcement = options.EscapeMarkdown ? EscapeMarkdown(clause.Enforcement) : clause.Enforcement;

        var headerPrefix = MakeHeaderPrefix(4 + options.HeaderLevelOffset);
        sb.AppendLine($"{headerPrefix} {clause.Id} {condition}");
        sb.AppendLine($"- {enforcement}");

        if (options.AddBlankLinesBetweenClauses)
        {
            sb.AppendLine();
        }
    }

    /// <summary>
    /// 更高效的 Markdown 转义：逐字符检查，必要时前置反斜杠
    /// </summary>
    private static string EscapeMarkdown(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var sb = new StringBuilder(text.Length * 2);

        foreach (var ch in text)
        {
            if (MarkdownSpecialChars.Contains(ch))
            {
                sb.Append('\\');
            }
            sb.Append(ch);
        }

        return sb.ToString();
    }

    private static string MakeHeaderPrefix(int level) => new string('#', Math.Max(1, level));

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");
}
