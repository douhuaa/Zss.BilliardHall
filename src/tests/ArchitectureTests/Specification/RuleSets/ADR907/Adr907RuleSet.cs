namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907：ArchitectureTests 执法治理体系规则集
/// 
/// 本规则集定义了 ArchitectureTests 的命名、组织、最小断言及 CI/Analyzer 映射规则，
/// 实现完整的自动裁决闭环。
/// 
/// 映射来源：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md (v2.1)
/// </summary>
public class Adr907RuleSet : ArchitectureRuleSet
{
    private static readonly Lazy<Adr907RuleSet> _instance = new(() => new Adr907RuleSet());

    /// <summary>
    /// 获取 ADR-907 规则集的单例实例
    /// </summary>
    public static Adr907RuleSet Instance => _instance.Value;

    /// <summary>
    /// 私有构造函数，确保单例模式
    /// </summary>
    private Adr907RuleSet()
    {
        AdrNumber = "ADR-907";
        Title = "ArchitectureTests 执法治理体系";
        Description = "整合 ArchitectureTests 的命名、组织、最小断言及 CI/Analyzer 映射规则，实现完整的自动裁决闭环";
        EnsureInitialized();
    }

    /// <summary>
    /// 使用 DSL 定义所有规则和条款
    /// </summary>
    protected override void DefineRules()
    {
        // ===================================================================
        // ADR-907_1：ArchitectureTests 的法律地位
        // ===================================================================
        this.Rule("ADR-907_1", "ArchitectureTests 的法律地位",
                "定义 ArchitectureTests 在架构治理中的权威地位和执法能力要求")

            .ConventionClause("1", "唯一自动化执法形式",
                "ArchitectureTests 是 ADR 的唯一自动化执法形式。允许使用文件系统扫描、Roslyn/Reflection，不强制使用 NetArchTest",
                "L1")

            .ConventionClause("2", "可执法性要求",
                "任何具备裁决力的 ADR 必须满足以下条件之一：已有对应的 ArchitectureTests 或明确声明为 Non-Enforceable",
                "L1")

            .ConventionClause("3", "禁止文档约束例外",
                "不存在声明为'文档专属、拒绝自动化'的架构规则",
                "L1");

        // ===================================================================
        // ADR-907_2：命名与组织规范（原 ADR-903）
        // ===================================================================
        this.Rule("ADR-907_2", "命名与组织规范",
                "定义 ArchitectureTests 的项目结构、命名约定和组织方式")

            .ConventionClause("1", "独立测试项目要求",
                "ArchitectureTests 必须集中于独立测试项目。项目命名格式：<SolutionName>.Tests.Architecture",
                "L1")

            .ConventionClause("2", "ADR 编号目录分组",
                "测试目录必须按 ADR 编号分组。目录格式：/ADR-XXXX/",
                "L1")

            .ConventionClause("3", "禁止跨 ADR 混合测试",
                "单个测试类或文件仅允许覆盖一个 ADR。每个测试类专注于单一 ADR 的约束验证",
                "L1")

            .ConventionClause("4", "测试类命名规范",
                "测试类命名必须显式绑定 ADR。命名格式：ADR_<编号>_Architecture_Tests",
                "L1")

            .ConventionClause("5", "测试方法命名规范",
                "测试方法必须映射 ADR 子规则。命名格式：ADR_<编号>_<子规则>_<行为描述>",
                "L1")

            .ConventionClause("6", "失败信息溯源要求",
                "测试失败信息必须包含 ADR 编号与子规则，支持从失败信息反向追溯到 ADR 条款",
                "L1")

            .ConventionClause("7", "禁止空弱断言",
                "ArchitectureTests 不得为空、占位或弱断言。每个测试必须包含有效的架构约束验证",
                "L1")

            .ConventionClause("8", "禁止跳过测试",
                "不得 Skip、条件禁用测试（除非走破例机制）。所有测试必须正常执行或通过破例流程处理",
                "L1");

        // ===================================================================
        // ADR-907_3：最小断言语义规范（原 ADR-904）
        // ===================================================================
        this.Rule("ADR-907_3", "最小断言语义规范",
                "定义 ArchitectureTests 中断言的最小数量和语义要求")

            .ConventionClause("1", "最小断言数量要求",
                "每个测试类至少包含 1 个有效断言。验证至少一个架构约束",
                "L1")

            .ConventionClause("2", "单一子规则映射",
                "每个测试方法只能映射一个 ADR 子规则。保持测试的单一职责和清晰性",
                "L1")

            .ConventionClause("3", "失败信息可溯源性",
                "所有断言失败信息必须可反向溯源到 ADR。包含 ADR 编号和具体条款引用",
                "L1")

            .ConventionClause("4", "禁止形式化断言",
                "明确禁止 Assert.True(true) 等形式化断言。仅验证测试可运行、不验证结构约束",
                "L1");

        // ===================================================================
        // ADR-907_4：Analyzer / CI Gate 映射协议（原 ADR-906）
        // ===================================================================
        this.Rule("ADR-907_4", "Analyzer / CI Gate 映射协议",
                "定义 ArchitectureTests 与 CI/Analyzer 的集成和映射规则")

            .ConventionClause("1", "自动发现注册要求",
                "所有 ArchitectureTests 必须被 Analyzer 自动发现并注册。支持 CI/CD 管道的自动执行",
                "L1")

            .ConventionClause("2", "RuleId 精确映射",
                "测试失败必须精确映射至 ADR 子规则（RuleId）。使用 ADR-907_<Rule>_<Clause> 格式标识",
                "L1")

            .ConventionClause("3", "执行级别分类支持",
                "支持执行级别分类（依赖 ADR-905）：L1 失败即阻断 CI/合并/部署；L2 失败记录告警，进入人工 Code Review",
                "L1")

            .ConventionClause("4", "破例机制自动记录",
                "破例机制必须自动记录：ADR 编号、测试类/方法、破例原因、到期时间与偿还计划",
                "L1")

            .ConventionClause("5", "Analyzer 检测能力要求",
                "Analyzer 必须具备检测能力：空测试/弱断言、单测试覆盖多 ADR、非 Final ADR 生成测试",
                "L1")

            .ConventionClause("6", "ADR 生命周期同步",
                "ADR 生命周期变更必须同步。Superseded/Obsolete ADR 对应测试必须标记或移除",
                "L1");
    }
}
