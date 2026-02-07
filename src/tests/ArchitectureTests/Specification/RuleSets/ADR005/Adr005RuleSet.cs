namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR005;

/// <summary>
/// ADR-005：业务逻辑分层架构
/// 定义 Handler 模式、模块通信机制和 CQRS 约束规则
/// </summary>
public sealed class Adr005RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 5;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(5);

        // Rule 1: Handler 命名与职责单一
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Handler 命名与职责单一",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "每个业务用例必须唯一 Handler",
            enforcement: "验证 Handler 命名明确表达业务意图（CommandHandler/QueryHandler/EventHandler）",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "Endpoint 仅做请求适配",
            enforcement: "验证 Endpoint/Controller 构造函数依赖不超过 5 个",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: Handler 无状态约束
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "Handler 无状态约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "Handler 不得持有业务状态",
            enforcement: "验证 Handler 无可变字段（非 readonly）",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "Handler 禁止作为跨模块粘合层",
            enforcement: "验证 Handler 不注入其他模块的类型（仅允许契约/事件总线）",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 3,
            condition: "Handler 不允许返回领域实体",
            enforcement: "验证 Handler 返回类型为 DTO/Contract，非领域实体",
            executionType: ClauseExecutionType.Convention);

        // Rule 3: 模块间通信约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "模块间通信约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "模块内允许同步调用",
            enforcement: "文档化模块内同步调用是允许的",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "模块间默认异步通信",
            enforcement: "验证跨模块通信仅通过事件总线/消息总线，禁止直接注入其他模块 Handler",
            executionType: ClauseExecutionType.Convention);

        // Rule 4: 契约通信约束
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "契约通信约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "模块间仅通过契约通信",
            enforcement: "验证领域实体不直接暴露给其他模块，仅通过 DTO/Contract",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 2,
            condition: "契约不承载业务决策",
            enforcement: "验证契约类型仅包含数据字段，无业务方法或复杂计算属性",
            executionType: ClauseExecutionType.Convention);

        // Rule 5: CQRS 分离约束
        ruleSet.AddRule(
            ruleNumber: 5,
            summary: "CQRS 分离约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 5,
            clauseNumber: 1,
            condition: "Command Handler 只执行业务逻辑",
            enforcement: "验证 CommandHandler 返回简单类型（ID/bool/void），不返回复杂业务对象",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 5,
            clauseNumber: 2,
            condition: "Query Handler 只读返回",
            enforcement: "验证 QueryHandler 返回 DTO/Contract，无写操作",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 5,
            clauseNumber: 3,
            condition: "Command/Query 必须分离",
            enforcement: "验证 Handler 命名明确区分 Command 和 Query",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
