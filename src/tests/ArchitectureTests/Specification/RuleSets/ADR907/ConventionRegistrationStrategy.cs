namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// 约定检查注册策略
/// 用于注册 Convention 类型的条款
/// 
/// Convention 类型的条款通过架构测试验证约定和规范
/// 适用于需要反射、类型检查等运行时验证的规则
/// </summary>
public sealed class ConventionRegistrationStrategy : IClauseRegistrationStrategy
{
    /// <summary>
    /// 注册约定检查条款
    /// </summary>
    public void Register(ArchitectureRuleSet ruleSet, ClauseSpec spec, ClauseExecutionBinding? binding)
    {
        spec.Validate();

        // 使用 ClauseSpec 的 Description 和 ValidationHint 注册条款
        // Convention 类型的条款使用现有的 AddClause API
        ruleSet.AddClause(
            ruleNumber: spec.RuleId,
            clauseNumber: spec.ClauseId,
            condition: spec.Description,
            enforcement: spec.ValidationHint,
            executionType: ClauseExecutionType.Convention);

        // 如果提供了 binding，可以在此处记录或处理（未来扩展点）
        // 当前约定检查主要基于命名和结构约定，不需要显式绑定到 Handler
    }
}
