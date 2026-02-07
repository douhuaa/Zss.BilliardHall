using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR002;

/// <summary>
/// ADR-002：Platform/Application/Host 启动引导
/// 定义应用启动、依赖注入、配置加载等规则
/// </summary>
public sealed class Adr002RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 2;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(2);

        // Rule 1: Platform 职责约束
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Platform 职责约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Platform 仅包含基础设施引导逻辑",
            enforcement: "验证 Platform 无业务逻辑",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "Platform 负责模块发现和注册",
            enforcement: "验证 PlatformBootstrapper 调用模块注册方法",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: 启动引导顺序
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "启动引导顺序",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "Host → Platform → Modules 的启动顺序",
            enforcement: "验证 Program.cs 调用顺序",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "禁止模块直接访问 Host 配置",
            enforcement: "验证模块无对 IConfiguration 的直接引用",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
