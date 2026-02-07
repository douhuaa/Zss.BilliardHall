namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR008;

/// <summary>
/// ADR-008：文档层级治理
/// 定义文档分级、裁决权归属和内容约束规则
/// </summary>
public sealed class Adr008RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 8;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(8);

        // Rule 1: 文档分级与裁决权
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "文档分级与裁决权",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "ADR 文档必须位于 adr 目录",
            enforcement: "验证所有 ADR 文档位于 docs/adr/ 目录下",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "只有 ADR 具备裁决力",
            enforcement: "验证 ADR 文档包含 Decision 章节",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "非 ADR 文档不得定义架构规则",
            enforcement: "验证 Guides/Skills 等文档不包含 MUST/MUST NOT 等强制规则",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 2: ADR 内容约束
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "ADR 内容约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "ADR 允许的内容范围",
            enforcement: "验证 ADR 包含架构决策、约束规则、判定标准",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "ADR 禁止的内容",
            enforcement: "验证 ADR 不包含实现细节、操作步骤、代码示例",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 3,
            condition: "ADR 内容判定规则",
            enforcement: "验证 ADR 决策清晰、可验证、可执行",
            executionType: ClauseExecutionType.Convention);

        // Rule 3: 非 ADR 文档约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "非 ADR 文档约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "Instructions/Agents 必须声明权威依据",
            enforcement: "验证 Agent 配置引用对应的 ADR 作为权威来源",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "Skills 不得输出判断性结论",
            enforcement: "验证 Skills 文档只提供信息，不做架构判定",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 3,
            condition: "README/Guide 必须声明无裁决力",
            enforcement: "验证指南文档明确声明仅供参考，以 ADR 为准",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 4: ADR 结构要求
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "ADR 结构要求",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Document);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "ADR 必需章节检查",
            enforcement: "验证 ADR 包含：Title、Status、Context、Decision、Consequences 章节",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
