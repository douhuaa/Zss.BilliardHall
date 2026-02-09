namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR340;

/// <summary>
/// ADR-340：结构化日志与监控约束
/// 定义 Platform 层日志依赖、配置约束和模块层隔离规则
/// </summary>
public sealed class Adr340RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 340;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(340);

        // Rule 1: Platform 层日志和监控依赖约束
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Platform 层日志和监控依赖约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Platform 层必须引用所有日志和监控基础设施包",
            enforcement: "验证 Platform.csproj 引用 Serilog、OpenTelemetry 等必要包",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "PlatformBootstrapper 必须包含日志配置代码",
            enforcement: "验证 PlatformBootstrapper.cs 存在并包含日志配置逻辑",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 5,
            condition: "Modules 层不应直接引用 Serilog 或 OpenTelemetry",
            enforcement: "验证模块层通过 ILogger 接口使用日志，不直接依赖具体实现",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
