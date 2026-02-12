namespace Zss.BilliardHall.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907 规则定义
/// 提供 ADR-907 的完整规则和条款定义
/// 
/// 设计说明：
/// - 使用 ClauseSpec 定义条款规范（声明式）
/// - 与 ClauseExecutionBinding 分离（执行绑定在 Adr907ExecutionBindings 中）
/// - RuleInfo 和 ClauseSpec 提供结构化的规则元数据
/// </summary>
public static class Adr907Definitions
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public const int AdrId = 907;

    /// <summary>
    /// 规则信息
    /// </summary>
    public sealed record RuleInfo(
        int RuleId,
        string Summary,
        DecisionLevel Decision,
        RuleSeverity Severity,
        RuleScope Scope,
        IReadOnlyList<ClauseSpec> Clauses);

    /// <summary>
    /// 所有规则定义
    /// </summary>
    public static IReadOnlyList<RuleInfo> AllRules { get; } = new[]
    {
        // Rule 1: ArchitectureTests 的法律地位
        new RuleInfo(
            RuleId: 1,
            Summary: "ArchitectureTests 的法律地位",
            Decision: DecisionLevel.Must,
            Severity: RuleSeverity.Governance,
            Scope: RuleScope.Test,
            Clauses: new[]
            {
                new ClauseSpec(
                    RuleId: 1,
                    ClauseId: 1,
                    Name: "唯一执法形式",
                    Description: "ArchitectureTests 是 ADR 的唯一自动化执法形式",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证 ArchitectureTests 作为唯一执法手段"),

                new ClauseSpec(
                    RuleId: 1,
                    ClauseId: 2,
                    Name: "必须有测试",
                    Description: "任何具备裁决力的 ADR 必须有对应的 ArchitectureTests 或明确声明为 Non-Enforceable",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "检测 Final ADR 是否具备对应测试或 Non-Enforceable 声明"),

                new ClauseSpec(
                    RuleId: 1,
                    ClauseId: 3,
                    Name: "禁止无执法路径",
                    Description: "不存在声明为'文档专属、拒绝自动化'的架构规则",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "禁止无执法路径的架构规则存在")
            }),

        // Rule 2: 命名与组织规范
        new RuleInfo(
            RuleId: 2,
            Summary: "命名与组织规范",
            Decision: DecisionLevel.Must,
            Severity: RuleSeverity.Governance,
            Scope: RuleScope.Test,
            Clauses: new[]
            {
                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 1,
                    Name: "独立测试项目",
                    Description: "ArchitectureTests 必须集中于独立测试项目",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证 ArchitectureTests 项目存在性"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 2,
                    Name: "按 ADR 分组",
                    Description: "测试目录必须按 ADR 编号分组",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证目录结构符合 /ADR-XXX/ 格式"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 3,
                    Name: "一对一映射",
                    Description: "单个测试类或文件仅允许覆盖一个 ADR",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "检查测试类与 ADR 映射的一致性"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 4,
                    Name: "显式绑定命名",
                    Description: "测试类命名必须显式绑定 ADR",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证命名格式：ADR_{编号}_{Rule}_Architecture_Tests"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 5,
                    Name: "方法映射子规则",
                    Description: "测试方法必须映射 ADR 子规则",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证命名格式：ADR_{编号}_{Rule}_{Clause}_{行为描述}"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 6,
                    Name: "失败信息溯源",
                    Description: "测试失败信息必须包含 ADR 编号与子规则",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证失败信息的 ADR 溯源能力"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 7,
                    Name: "禁止弱断言",
                    Description: "ArchitectureTests 不得为空、占位或弱断言",
                    ExecutionType: ClauseExecutionType.StaticAnalysis,
                    ValidationHint: "检测空测试和弱断言"),

                new ClauseSpec(
                    RuleId: 2,
                    ClauseId: 8,
                    Name: "禁止跳过测试",
                    Description: "不得 Skip、条件禁用测试（除非走破例机制）",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "检测 Skip 和条件编译指令")
            }),

        // Rule 3: 最小断言语义规范
        new RuleInfo(
            RuleId: 3,
            Summary: "最小断言语义规范",
            Decision: DecisionLevel.Must,
            Severity: RuleSeverity.Governance,
            Scope: RuleScope.Test,
            Clauses: new[]
            {
                new ClauseSpec(
                    RuleId: 3,
                    ClauseId: 1,
                    Name: "最小断言数量",
                    Description: "每个测试类至少包含1个有效断言",
                    ExecutionType: ClauseExecutionType.StaticAnalysis,
                    ValidationHint: "通过静态分析验证断言数量"),

                new ClauseSpec(
                    RuleId: 3,
                    ClauseId: 2,
                    Name: "单一职责",
                    Description: "每个测试方法只能映射一个ADR子规则",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "通过命名模式检查验证单一职责"),

                new ClauseSpec(
                    RuleId: 3,
                    ClauseId: 3,
                    Name: "可溯源失败",
                    Description: "所有断言失败信息必须可反向溯源到ADR",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证失败消息包含ADR引用、违规标记、修复建议和文档引用"),

                new ClauseSpec(
                    RuleId: 3,
                    ClauseId: 4,
                    Name: "禁止形式化",
                    Description: "禁止形式化断言",
                    ExecutionType: ClauseExecutionType.StaticAnalysis,
                    ValidationHint: "禁止 Assert.True(true) 等无意义断言")
            }),

        // Rule 4: Analyzer / CI Gate 映射协议
        new RuleInfo(
            RuleId: 4,
            Summary: "Analyzer / CI Gate 映射协议",
            Decision: DecisionLevel.Must,
            Severity: RuleSeverity.Governance,
            Scope: RuleScope.Test,
            Clauses: new[]
            {
                new ClauseSpec(
                    RuleId: 4,
                    ClauseId: 1,
                    Name: "自动发现",
                    Description: "所有 ArchitectureTests 必须被 Analyzer 自动发现并注册",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证测试的可发现性和注册机制"),

                new ClauseSpec(
                    RuleId: 4,
                    ClauseId: 2,
                    Name: "RuleId 格式",
                    Description: "测试失败必须精确映射至 ADR 子规则（RuleId）",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证 RuleId 格式为 ADR-XXX_Y_Z"),

                new ClauseSpec(
                    RuleId: 4,
                    ClauseId: 3,
                    Name: "执行级别",
                    Description: "支持执行级别分类（L1/L2）",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证 L1 阻断和 L2 告警策略"),

                new ClauseSpec(
                    RuleId: 4,
                    ClauseId: 4,
                    Name: "破例记录",
                    Description: "破例机制必须自动记录",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证破例的 ADR 编号、测试类/方法、原因、到期时间和偿还计划"),

                new ClauseSpec(
                    RuleId: 4,
                    ClauseId: 5,
                    Name: "Analyzer 检测",
                    Description: "Analyzer 必须具备检测能力",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证能检测空测试/弱断言/跨ADR/非Final ADR生成测试"),

                new ClauseSpec(
                    RuleId: 4,
                    ClauseId: 6,
                    Name: "生命周期同步",
                    Description: "ADR 生命周期变更必须同步",
                    ExecutionType: ClauseExecutionType.Convention,
                    ValidationHint: "验证 Superseded/Obsolete ADR 对应测试的处理")
            })
    };
}
