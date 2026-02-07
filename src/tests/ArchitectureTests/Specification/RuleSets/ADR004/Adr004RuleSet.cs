namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR004;

/// <summary>
/// ADR-004：Central Package Management (CPM) 规范
/// 定义集中式包管理的基础设施、依赖管理和层级约束规则
/// </summary>
public sealed class Adr004RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 4;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(4);

        // Rule 1: CPM 基础设施约束
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "CPM 基础设施约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Directory.Packages.props 必须存在",
            enforcement: "验证仓库根目录存在 Directory.Packages.props 文件",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "CPM 必须启用",
            enforcement: "验证 Directory.Packages.props 包含 ManagePackageVersionsCentrally=true",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "传递依赖固定建议启用",
            enforcement: "建议启用 CentralPackageTransitivePinningEnabled 以固定传递依赖版本",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 2: 项目依赖管理约束
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "项目依赖管理约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "项目文件禁止手动指定包版本",
            enforcement: "验证项目文件中的 PackageReference 不包含 Version 属性",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "所有使用的包必须在 CPM 中定义",
            enforcement: "验证项目引用的包在 Directory.Packages.props 中有对应版本定义",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 3: 层级依赖与分组约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "层级依赖与分组约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "包分组必须按用途组织",
            enforcement: "验证 Directory.Packages.props 中包按功能分组（测试、基础设施等）",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "包版本应统一管理",
            enforcement: "验证相同包在不同项目中使用统一版本",
            executionType: ClauseExecutionType.StaticAnalysis);

        return ruleSet;
    });
}
