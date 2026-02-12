namespace Zss.BilliardHall.Specification.RuleSets.ADR907;

/// <summary>
/// 静态分析注册策略
/// 用于注册 StaticAnalysis 类型的条款
/// 
/// StaticAnalysis 类型的条款通过编译时分析验证（如 Roslyn Analyzer）
/// 适用于代码结构、命名规范等可在编译时检查的规则
/// </summary>
public sealed class StaticAnalysisRegistrationStrategy : IClauseRegistrationStrategy
{
    /// <summary>
    /// 注册静态分析条款
    /// </summary>
    public void Register(ArchitectureRuleSet ruleSet, ClauseSpec spec, ClauseExecutionBinding? binding)
    {
        spec.Validate();

        // 使用 ClauseSpec 的 Description 和 ValidationHint 注册条款
        ruleSet.AddClause(
            ruleNumber: spec.RuleId,
            clauseNumber: spec.ClauseId,
            condition: spec.Description,
            enforcement: spec.ValidationHint,
            executionType: ClauseExecutionType.StaticAnalysis);

        // 如果提供了 binding，可以用于链接到具体的 Analyzer（未来扩展点）
        // 如: binding.HandlerKey = "Analyzer.Detect.WeakAssertions"
    }
}
