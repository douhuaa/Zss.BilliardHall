namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR007;

/// <summary>
/// ADR-007：Agent 行为与权限宪法
/// 定义 Agent 定位、判定规则和权限边界约束
/// </summary>
public sealed class Adr007RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 7;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(7);

        // Rule 1: Agent 根本定位
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Agent 根本定位",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Agent 定位必须为工具",
            enforcement: "验证 Agent 配置文件不包含声称裁决权的表述",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "Agent 必须声明权威边界",
            enforcement: "验证 Agent 配置明确引用 ADR 作为权威来源",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "Agent 禁止批准架构破例",
            enforcement: "验证 Agent 配置不声称可批准破例",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 2: 三态判定规则
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "三态判定规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "Agent 响应必须包含三态标识",
            enforcement: "验证 Agent 输出使用 Allowed/Blocked/Uncertain 三态标识",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "无法确认时必须假定禁止",
            enforcement: "验证 Agent 对不确定情况标记为 Uncertain 而非 Allowed",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 3,
            condition: "禁止输出模糊判断",
            enforcement: "验证 Agent 不使用'可能'、'建议'等模糊表述作为最终判定",
            executionType: ClauseExecutionType.Convention);

        // Rule 3: 权限边界约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "权限边界约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "Agent 禁止解释性扩权",
            enforcement: "验证 Agent 不扩展 ADR 正文未明确的规则",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "Agent 禁止替代性裁决",
            enforcement: "验证 Guardian Agent 不替代 Specialist 做最终裁决",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 3,
            condition: "Agent 禁止模糊输出",
            enforcement: "验证 Agent 输出明确且可验证",
            executionType: ClauseExecutionType.Convention);

        // Rule 4: 配置文件声明要求
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "配置文件声明要求",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "Prompts 文件必须声明无裁决力",
            enforcement: "验证 Agent 配置文件明确声明 ADR 为唯一权威来源",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
