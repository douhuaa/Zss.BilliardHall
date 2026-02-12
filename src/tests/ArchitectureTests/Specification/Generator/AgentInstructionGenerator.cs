namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// Agent 指令生成器实现
/// 将 ArchitectureRuleSet 转换为 YAML 格式的 Agent Instructions
/// </summary>
public sealed class AgentInstructionGenerator : IAgentInstructionGenerator
{
    /// <summary>
    /// 从 RuleSet 生成 YAML 格式的 Agent Instructions（使用默认选项）
    /// </summary>
    public string GenerateInstructions(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        return GenerateInstructions(ruleSet, InstructionGenerationOptions.Default);
    }

    /// <summary>
    /// 从 RuleSet 生成 YAML 格式的 Agent Instructions（带选项）
    /// </summary>
    public string GenerateInstructions(ArchitectureRuleSet ruleSet, InstructionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        var sb = new StringBuilder();
        var indent = new string(' ', options.IndentSpaces);

        // YAML 文件头部
        sb.AppendLine("instructions:");

        var orderedRules = ruleSet.Rules
            .OrderBy(r => r.Id.RuleNumber)
            .ToList();

        if (!orderedRules.Any())
        {
            return NormalizeNewlines(sb.ToString());
        }

        int instructionNumber = options.StartInstructionNumber;

        // 为每个 Rule 生成一个指令
        for (int i = 0; i < orderedRules.Count; i++)
        {
            var rule = orderedRules[i];
            AppendRuleInstruction(sb, rule, ruleSet, options, instructionNumber, indent);
            
            if (i < orderedRules.Count - 1)
            {
                sb.AppendLine();
            }
            
            instructionNumber++;
        }

        return NormalizeNewlines(sb.ToString());
    }

    private static void AppendRuleInstruction(
        StringBuilder sb,
        ArchitectureRuleDefinition rule,
        ArchitectureRuleSet ruleSet,
        InstructionGenerationOptions options,
        int instructionNumber,
        string indent)
    {
        if (rule is null)
            return;

        var instructionId = $"{options.AgentPrefix}-{instructionNumber:D3}";
        var clauses = GetClausesForRule(rule, ruleSet);

        // ID
        sb.AppendLine($"{indent}- id: {instructionId}");

        // Description
        var description = EscapeYamlString(rule.Summary);
        sb.AppendLine($"{indent}  description: \"{description}\"");

        // Action
        var action = GenerateAction(rule, clauses);
        sb.AppendLine($"{indent}  action: \"{action}\"");

        // Conditions
        sb.AppendLine($"{indent}  conditions:");
        AppendConditions(sb, rule, indent);

        // Output
        var output = GenerateOutput(rule);
        sb.AppendLine($"{indent}  output: \"{output}\"");

        // Tools
        sb.AppendLine($"{indent}  tools:");
        AppendTools(sb, rule, ruleSet, indent);

        // Feedback
        sb.AppendLine($"{indent}  feedback:");
        AppendFeedback(sb, rule, indent);

        // Guidelines (可选)
        if (options.IncludeGuidelines)
        {
            sb.AppendLine($"{indent}  guidelines:");
            AppendGuidelines(sb, rule, clauses, indent);
        }

        // Commands (可选)
        if (options.IncludeTestCommands)
        {
            sb.AppendLine($"{indent}  commands:");
            AppendCommands(sb, ruleSet, indent);
        }
    }

    private static List<ArchitectureClauseDefinition> GetClausesForRule(
        ArchitectureRuleDefinition rule,
        ArchitectureRuleSet ruleSet)
    {
        if (rule is null || ruleSet is null)
            return new List<ArchitectureClauseDefinition>();

        return ruleSet.Clauses
            .Where(c => c.Id.AdrNumber == rule.Id.AdrNumber && c.Id.RuleNumber == rule.Id.RuleNumber)
            .OrderBy(c => c.Id.ClauseNumber)
            .ToList();
    }

    private static string GenerateAction(ArchitectureRuleDefinition rule, List<ArchitectureClauseDefinition> clauses)
    {
        var clauseCount = clauses.Count;
        var action = $"验证 {rule.Id} 的 {clauseCount} 个约束条款";
        return EscapeYamlString(action);
    }

    private static void AppendConditions(StringBuilder sb, ArchitectureRuleDefinition rule, string indent)
    {
        sb.AppendLine($"{indent}    - \"PullRequest\"");
        
        if (rule.Scope == RuleScope.Solution)
        {
            sb.AppendLine($"{indent}    - \"CI pipeline\"");
        }
        else if (rule.Scope == RuleScope.Module)
        {
            sb.AppendLine($"{indent}    - \"Code Modified\"");
        }
        else if (rule.Scope == RuleScope.Document)
        {
            sb.AppendLine($"{indent}    - \"Documentation Updated\"");
        }
        else if (rule.Scope == RuleScope.Test)
        {
            sb.AppendLine($"{indent}    - \"Test Modified\"");
        }
        else if (rule.Scope == RuleScope.Agent)
        {
            sb.AppendLine($"{indent}    - \"Agent Instruction Updated\"");
        }
    }

    private static string GenerateOutput(ArchitectureRuleDefinition rule)
    {
        return "Allowed / Blocked / Uncertain";
    }

    private static void AppendTools(StringBuilder sb, ArchitectureRuleDefinition rule, ArchitectureRuleSet ruleSet, string indent)
    {
        sb.AppendLine($"{indent}    - \"RuleSet API\"");
        sb.AppendLine($"{indent}    - \"ArchitectureTests\"");
        sb.AppendLine($"{indent}    - \"ADR-{ruleSet.AdrNumber} RuleSet\"");
    }

    private static void AppendFeedback(StringBuilder sb, ArchitectureRuleDefinition rule, string indent)
    {
        sb.AppendLine($"{indent}    - \"生成 FailureObject（如违反约束）\"");
        
        if (rule.Severity == RuleSeverity.Constitutional)
        {
            sb.AppendLine($"{indent}    - \"阻断 CI 管道（Constitutional 级别）\"");
        }
        else if (rule.Severity == RuleSeverity.Governance)
        {
            sb.AppendLine($"{indent}    - \"阻止 PR 合并（Governance 级别）\"");
        }
        else if (rule.Severity == RuleSeverity.Technical)
        {
            sb.AppendLine($"{indent}    - \"生成架构警告（Technical 级别）\"");
        }
        
        sb.AppendLine($"{indent}    - \"记录违规到日志\"");
    }

    private static void AppendGuidelines(
        StringBuilder sb,
        ArchitectureRuleDefinition rule,
        List<ArchitectureClauseDefinition> clauses,
        string indent)
    {
        // RuleSet API 查询示例
        sb.AppendLine($"{indent}    - \"RuleSet API 查询示例：\"");
        
        foreach (var clause in clauses)
        {
            var example = GenerateApiQueryExample(clause);
            sb.AppendLine($"{indent}      - \"{EscapeYamlString(example)}\"");
        }

        // 约束检查逻辑
        sb.AppendLine($"{indent}    - \"约束检查逻辑：\"");
        
        foreach (var clause in clauses)
        {
            var checkLogic = GenerateConstraintCheckLogic(clause);
            sb.AppendLine($"{indent}      - \"{EscapeYamlString(checkLogic)}\"");
        }
    }

    private static void AppendCommands(StringBuilder sb, ArchitectureRuleSet ruleSet, string indent)
    {
        var adrNumber = ruleSet.AdrNumber;
        
        sb.AppendLine($"{indent}    run_adr_tests: \"dotnet test src/tests/ArchitectureTests/ --filter \\\"FullyQualifiedName~ADR{adrNumber:D3}\\\" --logger \\\"console;verbosity=detailed\\\"\"");
        sb.AppendLine($"{indent}    run_all_architecture_tests: \"dotnet test src/tests/ArchitectureTests/ --filter \\\"Category=Architecture\\\" --logger \\\"console;verbosity=detailed\\\"\"");
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

    /// <summary>
    /// 转义 YAML 字符串中的特殊字符
    /// </summary>
    private static string EscapeYamlString(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // 转义双引号
        return text.Replace("\"", "\\\"");
    }

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");
}
