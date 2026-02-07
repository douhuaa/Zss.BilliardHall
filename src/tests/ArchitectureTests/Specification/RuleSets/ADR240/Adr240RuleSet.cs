namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR240;

/// <summary>
/// ADR-240：Handler 异常约束
/// 定义异常分类、可重试标记和命名空间约束规则
/// </summary>
public sealed class Adr240RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 240;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(240);

        // Rule 1: 结构化异常要求
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "结构化异常要求",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Runtime,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Handler 仅抛出结构化异常",
            enforcement: "验证所有自定义异常继承自 DomainException、ValidationException 或 InfrastructureException",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: 可重试标记约束
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "可重试标记约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Runtime,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "可重试异常必须是基础设施异常",
            enforcement: "验证标记为可重试的异常继承自 InfrastructureException，领域异常和验证异常不可标记为可重试",
            executionType: ClauseExecutionType.Convention);

        // Rule 4: 异常命名空间约束
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "异常命名空间约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Runtime,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "异常类型必须在正确的命名空间",
            enforcement: "验证异常类型位于 *.Exceptions.* 命名空间，按类型分类组织",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
