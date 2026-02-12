namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// 文档验证注册策略
/// 用于注册 Documentation 类型的条款
/// 
/// Documentation 类型的条款通过文档格式、内容、链接等验证
/// 适用于文档质量、ADR 格式等规则
/// </summary>
public sealed class DocumentationRegistrationStrategy : IClauseRegistrationStrategy
{
    /// <summary>
    /// 注册文档验证条款
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
            executionType: ClauseExecutionType.Documentation);

        // 如果提供了 binding，可以用于链接到文档验证工具（未来扩展点）
    }
}
