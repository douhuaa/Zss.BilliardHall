using Zss.BilliardHall.Generators.Models;
using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Specification.Language.RuleIdLanguage;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// 指令模型构建器
/// 负责从 RuleSet 构建 InstructionModel 对象
/// </summary>
internal sealed class InstructionModelBuilder
{
    private readonly InstructionGenerationOptions _options;
    private readonly Dictionary<(int AdrNumber, int RuleNumber), List<ArchitectureClauseDefinition>> _clausesCache;

    public InstructionModelBuilder(
        ArchitectureRuleSet ruleSet,
        InstructionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _clausesCache = BuildClausesCache(ruleSet);
    }

    /// <summary>
    /// 构建 Clauses 缓存（分组查找，优化性能）
    /// </summary>
    private static Dictionary<(int AdrNumber, int RuleNumber), List<ArchitectureClauseDefinition>> BuildClausesCache(
        ArchitectureRuleSet ruleSet)
    {
        return ruleSet.Clauses
            .GroupBy(c => (c.Id.AdrNumber, c.Id.RuleNumber))
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.Id.ClauseNumber).ToList());
    }

    /// <summary>
    /// 从 Rule 构建 InstructionModel
    /// </summary>
    public InstructionModel BuildInstruction(
        ArchitectureRuleDefinition rule,
        ArchitectureRuleSet ruleSet,
        int instructionNumber)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(ruleSet);

        var instructionId = $"{_options.AgentPrefix}-{instructionNumber:D3}";
        var clauses = GetClausesForRule(rule);

        return new InstructionModel
        {
            Id = instructionId,
            Description = rule.Summary ?? string.Empty,
            Action = GenerateAction(rule, clauses),
            Conditions = GenerateConditions(rule),
            Output = "Allowed / Blocked / Uncertain",
            Tools = GenerateTools(rule, ruleSet),
            Feedback = GenerateFeedback(rule),
            Guidelines = _options.IncludeGuidelines ? GenerateGuidelines(rule, clauses) : null,
            Commands = _options.IncludeTestCommands ? GenerateCommands(ruleSet) : null
        };
    }

    private List<ArchitectureClauseDefinition> GetClausesForRule(ArchitectureRuleDefinition rule)
    {
        var key = (rule.Id.AdrNumber, rule.Id.RuleNumber);
        return _clausesCache.TryGetValue(key, out var clauses)
            ? clauses
            : new List<ArchitectureClauseDefinition>();
    }

    private static string GenerateAction(ArchitectureRuleDefinition rule, List<ArchitectureClauseDefinition> clauses)
    {
        var clauseCount = clauses.Count;
        return $"验证 {rule.Id} 的 {clauseCount} 个约束条款";
    }

    private static List<string> GenerateConditions(ArchitectureRuleDefinition rule)
    {
        var conditions = new List<string> { "PullRequest" };

        var scopeCondition = rule.Scope switch
        {
            RuleScope.Solution => "CI pipeline",
            RuleScope.Module => "Code Modified",
            RuleScope.Document => "Documentation Updated",
            RuleScope.Test => "Test Modified",
            RuleScope.Agent => "Agent Instruction Updated",
            _ => null
        };

        if (scopeCondition != null)
        {
            conditions.Add(scopeCondition);
        }

        return conditions;
    }

    private static List<string> GenerateTools(ArchitectureRuleDefinition rule, ArchitectureRuleSet ruleSet)
    {
        return new List<string>
        {
            "RuleSet API",
            "ArchitectureTests",
            $"ADR-{ruleSet.AdrNumber} RuleSet"
        };
    }

    private static List<string> GenerateFeedback(ArchitectureRuleDefinition rule)
    {
        var feedback = new List<string> { "生成 FailureObject（如违反约束）" };

        var severityFeedback = rule.Severity switch
        {
            RuleSeverity.Constitutional => "阻断 CI 管道（Constitutional 级别）",
            RuleSeverity.Governance => "阻止 PR 合并（Governance 级别）",
            RuleSeverity.Technical => "生成架构警告（Technical 级别）",
            _ => null
        };

        if (severityFeedback != null)
        {
            feedback.Add(severityFeedback);
        }

        feedback.Add("记录违规到日志");
        return feedback;
    }

    private static List<string> GenerateGuidelines(
        ArchitectureRuleDefinition rule,
        List<ArchitectureClauseDefinition> clauses)
    {
        var guidelines = new List<string> { "RuleSet API 查询示例：" };

        foreach (var clause in clauses)
        {
            var example = GenerateApiQueryExample(clause);
            guidelines.Add($"  - {example}");
        }

        guidelines.Add("约束检查逻辑：");

        foreach (var clause in clauses)
        {
            var checkLogic = GenerateConstraintCheckLogic(clause);
            guidelines.Add($"  - {checkLogic}");
        }

        return guidelines;
    }

    private static Dictionary<string, string> GenerateCommands(ArchitectureRuleSet ruleSet)
    {
        var adrNumber = ruleSet.AdrNumber;

        return new Dictionary<string, string>
        {
            ["run_adr_tests"] = $"dotnet test src/tests/ArchitectureTests/ --filter \\\"FullyQualifiedName~ADR{adrNumber:D3}\\\" --logger \\\"console;verbosity=detailed\\\"",
            ["run_all_architecture_tests"] = "dotnet test src/tests/ArchitectureTests/ --filter \\\"Category=Architecture\\\" --logger \\\"console;verbosity=detailed\\\""
        };
    }

    private static string GenerateApiQueryExample(ArchitectureClauseDefinition clause)
    {
        return $"ruleSet.GetClause({clause.Id.RuleNumber}, {clause.Id.ClauseNumber}) → {clause.Condition}";
    }

    private static string GenerateConstraintCheckLogic(ArchitectureClauseDefinition clause)
    {
        var executionType = clause.ExecutionType switch
        {
            ClauseExecutionType.StaticAnalysis => "使用静态分析验证",
            ClauseExecutionType.Convention => "检查约定遵守情况",
            ClauseExecutionType.Runtime => "运行时检查",
            ClauseExecutionType.Documentation => "文档验证",
            ClauseExecutionType.ManualReview => "需要人工审查",
            _ => "验证执行"
        };

        return $"{clause.Id} - {executionType}: {clause.Enforcement}";
    }
}
