namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR121;

/// <summary>
/// ADR-121：契约（Contract）与 DTO 命名组织规范
/// 定义跨模块契约类型的命名、不可变性、命名空间组织规则
/// </summary>
public sealed class Adr121RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 121;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(121);

        // Rule 1: 契约命名规则
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "契约命名规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "契约类型必须以 Dto 或 Contract 结尾",
            enforcement: "验证位于 Contracts 命名空间的类型以 Dto 或 Contract 后缀结尾",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: 契约不可变性
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "契约不可变性",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "契约属性必须是只读的",
            enforcement: "验证契约属性使用 record 或 init-only 修饰符",
            executionType: ClauseExecutionType.Convention);

        // Rule 3: 契约行为约束
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "契约行为约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "契约不得包含业务方法",
            enforcement: "验证契约类型仅包含数据属性，无业务逻辑方法",
            executionType: ClauseExecutionType.Convention);

        // Rule 4: 契约类型约束
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "契约类型约束",
            decision: DecisionLevel.MustNot,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "契约不得包含领域模型类型",
            enforcement: "验证契约属性不引用领域实体、聚合根、值对象",
            executionType: ClauseExecutionType.Convention);

        // Rule 5: 契约命名空间约束
        ruleSet.AddRule(
            ruleNumber: 5,
            summary: "契约命名空间约束",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 5,
            clauseNumber: 1,
            condition: "契约必须位于 Contracts 命名空间下",
            enforcement: "验证契约类型位于 *.Contracts.* 命名空间",
            executionType: ClauseExecutionType.Convention);

        // Rule 6: 契约物理组织
        ruleSet.AddRule(
            ruleNumber: 6,
            summary: "契约物理组织",
            decision: DecisionLevel.Should,
            severity: RuleSeverity.Structure,
            scope: RuleScope.Type);

        ruleSet.AddClause(
            ruleNumber: 6,
            clauseNumber: 1,
            condition: "契约命名空间应与物理目录一致",
            enforcement: "验证契约命名空间路径与文件系统目录结构一致（L2 建议）",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
