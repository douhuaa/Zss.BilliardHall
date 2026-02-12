using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.RuleIdLanguage;

namespace Zss.BilliardHall.AdrDecisionGenerator;

/// <summary>
/// ADR Decision 生成器实现
/// 
/// 重构目标：
/// - 提高可读性和可测试性
/// - 遵循单一职责原则（SRP）
/// - 采用早期返回减少嵌套
/// - 防御式编程，增强输入验证
/// - 避免重复计算，优化性能
/// 
/// 兼容性保证：
/// - 保持所有现有公共 API 签名不变
/// - 输出格式与 golden 示例完全一致
/// - 所有现有测试继续通过
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
            BuildSectionHeader(sb, options);
        }

        BuildRulesContent(sb, ruleSet, options);

        // 统一行尾为 LF，避免跨平台差异
        return NormalizeNewlines(sb.ToString());
    }

    /// <summary>
    /// 构建章节标题（Decision）
    /// </summary>
    private static void BuildSectionHeader(StringBuilder sb, DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(options);

        var headerPrefix = MakeHeaderPrefix(2 + options.HeaderLevelOffset);
        sb.AppendLine($"{headerPrefix} Decision（裁决）");
        sb.AppendLine();

        if (options.IncludeWarningNote)
        {
            sb.AppendLine("> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**");
            sb.AppendLine();
        }
    }

    /// <summary>
    /// 构建所有规则内容
    /// </summary>
    private static void BuildRulesContent(StringBuilder sb, ArchitectureRuleSet ruleSet, DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(options);

        // 早期返回：如果没有规则，直接返回
        if (!ruleSet.Rules.Any())
        {
            return;
        }

        var orderedRules = GetOrderedRules(ruleSet);

        // 性能优化：一次性构建按 RuleNumber 分组的条款字典
        // 复杂度从 O(N*M) 降低为 O(M log M + N)
        var clausesByRule = ruleSet.Clauses
            .Where(c => c.Id.AdrNumber == ruleSet.AdrNumber)
            .GroupBy(c => c.Id.RuleNumber)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.Id.ClauseNumber).ToList());

        for (int i = 0; i < orderedRules.Count; i++)
        {
            var clauses = clausesByRule.TryGetValue(orderedRules[i].Id.RuleNumber, out var list)
                ? list
                : new List<ArchitectureClauseDefinition>();

            BuildRuleSection(sb, orderedRules[i], clauses, options);

            // 规则之间添加空行（除了最后一个）
            if (i < orderedRules.Count - 1)
            {
                sb.AppendLine();
            }
        }
    }

    /// <summary>
    /// 获取排序后的规则列表
    /// </summary>
    private static List<ArchitectureRuleDefinition> GetOrderedRules(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);

        return ruleSet.Rules
            .OrderBy(r => r.Id.RuleNumber)
            .ToList();
    }

    /// <summary>
    /// 构建单个规则章节（带预处理的条款列表）
    /// </summary>
    private static void BuildRuleSection(
        StringBuilder sb,
        ArchitectureRuleDefinition rule,
        List<ArchitectureClauseDefinition> clauses,
        DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(clauses);
        ArgumentNullException.ThrowIfNull(options);

        // 早期返回：如果规则为空，直接返回
        if (rule is null)
        {
            return;
        }

        BuildRuleHeader(sb, rule, options);
        BuildClausesForRule(sb, clauses, options);
    }

    /// <summary>
    /// 构建规则标题
    /// </summary>
    private static void BuildRuleHeader(StringBuilder sb, ArchitectureRuleDefinition rule, DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(options);

        var ruleIdText = rule.Id.ToString();
        var summary = options.EscapeMarkdown ? EscapeMarkdown(rule.Summary) : rule.Summary;
        var headerPrefix = MakeHeaderPrefix(3 + options.HeaderLevelOffset);
        
        sb.AppendLine($"{headerPrefix} {ruleIdText}：{summary}（Rule）");
        sb.AppendLine();
    }

    /// <summary>
    /// 构建规则的所有条款（使用预处理的条款列表）
    /// </summary>
    private static void BuildClausesForRule(
        StringBuilder sb,
        List<ArchitectureClauseDefinition> clauses,
        DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(clauses);
        ArgumentNullException.ThrowIfNull(options);

        foreach (var clause in clauses)
        {
            BuildClauseSection(sb, clause, options);
        }
    }

    /// <summary>
    /// 构建单个条款章节
    /// </summary>
    private static void BuildClauseSection(
        StringBuilder sb,
        ArchitectureClauseDefinition clause,
        DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(options);

        // 早期返回：如果条款为空，直接返回
        if (clause is null)
        {
            return;
        }

        var clauseId = clause.Id.ToString();
        var condition = options.EscapeMarkdown ? EscapeMarkdown(clause.Condition) : clause.Condition;
        var enforcement = options.EscapeMarkdown ? EscapeMarkdown(clause.Enforcement) : clause.Enforcement;
        var headerPrefix = MakeHeaderPrefix(4 + options.HeaderLevelOffset);

        sb.AppendLine($"{headerPrefix} {clauseId} {condition}");
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
        // 早期返回：如果为空，返回空字符串
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

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

    /// <summary>
    /// 生成 Markdown 标题前缀（# 号）
    /// </summary>
    private static string MakeHeaderPrefix(int level) => new string('#', Math.Max(1, level));

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");
}
