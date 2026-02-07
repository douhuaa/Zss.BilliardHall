namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR201;

/// <summary>
/// ADR-201：Handler 生命周期管理
/// 定义 Handler 的创建、执行、释放等规则
/// </summary>
public sealed class Adr0201RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 201;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(201);

        // Rule 1: Handler 注册规范
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Handler 注册规范",
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Handler 必须通过 DI 容器注册",
            enforcement: "验证 Handler 类型已注册到 IServiceCollection");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "Handler 生命周期必须为 Scoped",
            enforcement: "验证 Handler 注册为 ServiceLifetime.Scoped");

        return ruleSet;
    });
}
