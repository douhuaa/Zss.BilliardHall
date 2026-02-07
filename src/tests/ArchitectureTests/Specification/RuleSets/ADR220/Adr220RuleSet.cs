namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR220;

/// <summary>
/// ADR-220：事件总线集成规范
/// 定义事件总线依赖隔离和订阅者生命周期规则
/// </summary>
public sealed class Adr220RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 220;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(220);

        // Rule 1: 事件总线依赖隔离
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "事件总线依赖隔离",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "模块禁止直接依赖具体事件总线实现",
            enforcement: "验证模块不依赖 Wolverine、RabbitMQ、Kafka 等具体实现，仅通过 IEventBus 接口",
            executionType: ClauseExecutionType.Convention);

        // Rule 4: 事件订阅者生命周期
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "事件订阅者生命周期",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "事件订阅者必须注册为 Scoped 或 Transient",
            enforcement: "验证所有 EventHandler 注册为 Scoped 或 Transient 生命周期",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
