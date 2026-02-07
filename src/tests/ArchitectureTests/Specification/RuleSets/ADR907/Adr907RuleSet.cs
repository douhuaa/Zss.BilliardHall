namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907：ArchitectureTests 执法治理体系
/// 定义架构测试的命名、组织、断言等规则
/// </summary>
public sealed class Adr907RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => 907;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(907);

        // Rule 1: ArchitectureTests 的法律地位
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "ArchitectureTests 的法律地位",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "ArchitectureTests 是 ADR 的唯一自动化执法形式",
            enforcement: "验证 ArchitectureTests 作为唯一执法手段",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "任何具备裁决力的 ADR 必须有对应的 ArchitectureTests 或明确声明为 Non-Enforceable",
            enforcement: "检测 Final ADR 是否具备对应测试或 Non-Enforceable 声明",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "不存在声明为'文档专属、拒绝自动化'的架构规则",
            enforcement: "禁止无执法路径的架构规则存在",
            executionType: ClauseExecutionType.Convention);

        // Rule 2: 命名与组织规范
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "命名与组织规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "ArchitectureTests 必须集中于独立测试项目",
            enforcement: "验证 ArchitectureTests 项目存在性",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "测试目录必须按 ADR 编号分组",
            enforcement: "验证目录结构符合 /ADR-XXX/ 格式",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 3,
            condition: "单个测试类或文件仅允许覆盖一个 ADR",
            enforcement: "检查测试类与 ADR 映射的一致性",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 4,
            condition: "测试类命名必须显式绑定 ADR",
            enforcement: "验证命名格式：ADR_{编号}_{Rule}_Architecture_Tests",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 5,
            condition: "测试方法必须映射 ADR 子规则",
            enforcement: "验证命名格式：ADR_{编号}_{Rule}_{Clause}_{行为描述}",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 6,
            condition: "测试失败信息必须包含 ADR 编号与子规则",
            enforcement: "验证失败信息的 ADR 溯源能力",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 7,
            condition: "ArchitectureTests 不得为空、占位或弱断言",
            enforcement: "检测空测试和弱断言",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 8,
            condition: "不得 Skip、条件禁用测试（除非走破例机制）",
            enforcement: "检测 Skip 和条件编译指令",
            executionType: ClauseExecutionType.Convention);

        // Rule 3: 最小断言语义规范
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "最小断言语义规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "每个测试类至少包含1个有效断言",
            enforcement: "通过静态分析验证断言数量",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "每个测试方法只能映射一个ADR子规则",
            enforcement: "通过命名模式检查验证单一职责",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 3,
            condition: "所有断言失败信息必须可反向溯源到ADR",
            enforcement: "验证失败消息包含ADR引用、违规标记、修复建议和文档引用",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 4,
            condition: "禁止形式化断言",
            enforcement: "禁止 Assert.True(true) 等无意义断言",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 4: Analyzer / CI Gate 映射协议
        ruleSet.AddRule(
            ruleNumber: 4,
            summary: "Analyzer / CI Gate 映射协议",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 1,
            condition: "所有 ArchitectureTests 必须被 Analyzer 自动发现并注册",
            enforcement: "验证测试的可发现性和注册机制",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 2,
            condition: "测试失败必须精确映射至 ADR 子规则（RuleId）",
            enforcement: "验证 RuleId 格式为 ADR-XXX_Y_Z",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 3,
            condition: "支持执行级别分类（L1/L2）",
            enforcement: "验证 L1 阻断和 L2 告警策略",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 4,
            condition: "破例机制必须自动记录",
            enforcement: "验证破例的 ADR 编号、测试类/方法、原因、到期时间和偿还计划",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 5,
            condition: "Analyzer 必须具备检测能力",
            enforcement: "验证能检测空测试/弱断言/跨ADR/非Final ADR生成测试",
            executionType: ClauseExecutionType.Convention);

        ruleSet.AddClause(
            ruleNumber: 4,
            clauseNumber: 6,
            condition: "ADR 生命周期变更必须同步",
            enforcement: "验证 Superseded/Obsolete ADR 对应测试的处理",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    });
}
