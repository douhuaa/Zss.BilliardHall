namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// 手工审查注册策略
/// 用于注册 ManualReview 类型的条款
/// 
/// ManualReview 类型的条款需要人工判断
/// 适用于主观性强、难以自动化的规则
/// 
/// 策略行为：
/// - 尝试注册为 ManualReview（如果 API 支持）
/// - 如果 API 不支持，降级为 Convention 并发出警告
/// </summary>
public sealed class ManualReviewRegistrationStrategy : IClauseRegistrationStrategy
{
    /// <summary>
    /// 注册手工审查条款
    /// </summary>
    public void Register(ArchitectureRuleSet ruleSet, ClauseSpec spec, ClauseExecutionBinding? binding)
    {
        spec.Validate();

        // 当前 ArchitectureRuleSet.AddClause 支持 ManualReview 类型
        // 如果未来 API 不支持，可以降级为 Convention
        try
        {
            ruleSet.AddClause(
                ruleNumber: spec.RuleId,
                clauseNumber: spec.ClauseId,
                condition: spec.Description,
                enforcement: spec.ValidationHint,
                executionType: ClauseExecutionType.ManualReview);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("ManualReview"))
        {
            // 如果 ManualReview 不被支持，降级为 Convention
            Trace.TraceWarning(
                $"[ADR-907] ManualReview 类型不被支持，条款 ({spec.RuleId},{spec.ClauseId}) 降级为 Convention。" +
                $"原因: {ex.Message}");

            ruleSet.AddClause(
                ruleNumber: spec.RuleId,
                clauseNumber: spec.ClauseId,
                condition: spec.Description,
                enforcement: spec.ValidationHint,
                executionType: ClauseExecutionType.Convention);
        }
    }
}
