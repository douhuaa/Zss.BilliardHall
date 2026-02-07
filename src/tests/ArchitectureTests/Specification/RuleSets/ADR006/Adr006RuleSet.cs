namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR006;

/// <summary>
/// ADR-006：ADR 编号层级规范
/// 定义 ADR 编号分层、格式标准和前导零规则
/// </summary>
public sealed class Adr006RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 6;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(6);

        // Rule 1: 编号分层规则
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "编号分层规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "编号段层级映射",
            enforcement: "验证 ADR 编号与目录层级匹配：宪法层(001-009)、结构层(100-199)、运行时层(200-299)、技术层(300-399)、治理层(900-999)",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 2: 编号格式规则
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "编号格式规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "标准编号格式",
            enforcement: "验证所有 ADR 编号为3位数字格式（000-999）",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "文件命名规范",
            enforcement: "验证文件命名格式：ADR-XXX-descriptive-title.md（小写字母、数字、连字符）",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 3: 前导零规则
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "前导零规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Documentation);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "前导零强制要求",
            enforcement: "验证小于100的 ADR 编号使用前导零（如 ADR-001, ADR-010）",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
