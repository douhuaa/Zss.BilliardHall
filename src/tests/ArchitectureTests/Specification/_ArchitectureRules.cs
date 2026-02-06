namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// ArchitectureTestSpecification - 架构规则集定义
/// 
/// 此部分类包含从 ADR 文档迁移而来的规则集定义
/// 每个 ADR 对应一个静态属性，返回完整的 ArchitectureRuleSet
/// 
/// 设计目标：
/// 1. 成为 ADR 内容的强类型镜像
/// 2. 作为架构测试的唯一规范源
/// 3. 支持自动化验证和测试生成
/// 4. 与 ADR 文档保持同步
/// 
/// 组织原则：
/// - 按 ADR 层级分组（Constitutional、Governance、Runtime、Structure、Technical）
/// - 每个 ADR 一个静态属性
/// - 使用惰性初始化（Lazy）提高性能
/// </summary>
public static partial class ArchitectureTestSpecification
{
    /// <summary>
    /// 架构规则集合
    /// 从 ADR 文档迁移而来的完整规则定义
    /// </summary>
    public static class ArchitectureRules
    {
        #region Constitutional ADRs (宪法层 ADR-001 ~ ADR-008)

        /// <summary>
        /// ADR-001：模块化单体与垂直切片架构
        /// 定义模块物理隔离、依赖方向、通信机制等核心规则
        /// </summary>
        public static ArchitectureRuleSet Adr001 => LazyAdr001.Value;
        
        private static readonly Lazy<ArchitectureRuleSet> LazyAdr001 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(1);

            // Rule 1: 模块物理隔离
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "模块物理隔离",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "模块按业务能力独立划分",
                enforcement: "通过 NetArchTest 验证模块不相互引用");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "项目文件禁止引用其他模块",
                enforcement: "解析 .csproj 文件验证无 ProjectReference 指向其他模块");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 3,
                condition: "命名空间匹配模块边界",
                enforcement: "验证类型命名空间与模块名称一致");

            // Rule 2: 垂直切片组织
            ruleSet.AddRule(
                ruleNumber: 2,
                summary: "垂直切片组织",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 1,
                condition: "每个模块包含完整的垂直切片",
                enforcement: "验证模块包含 Domain、Application、Infrastructure 层次");

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 2,
                condition: "禁止跨模块水平分层",
                enforcement: "验证无跨模块的 Domain/Application 层依赖");

            // Rule 3: 模块通信机制
            ruleSet.AddRule(
                ruleNumber: 3,
                summary: "模块通信机制",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 1,
                condition: "模块间仅通过领域事件异步通信",
                enforcement: "验证无直接方法调用，仅事件发布/订阅");

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 2,
                condition: "模块间查询仅通过数据契约",
                enforcement: "验证查询使用只读 DTO，无领域对象传递");

            return ruleSet;
        });

        /// <summary>
        /// ADR-002：Platform/Application/Host 启动引导
        /// 定义应用启动、依赖注入、配置加载等规则
        /// </summary>
        public static ArchitectureRuleSet Adr002 => LazyAdr002.Value;

        private static readonly Lazy<ArchitectureRuleSet> LazyAdr002 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(2);

            // Rule 1: Platform 职责约束
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "Platform 职责约束",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Solution);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "Platform 仅包含基础设施引导逻辑",
                enforcement: "验证 Platform 无业务逻辑");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "Platform 负责模块发现和注册",
                enforcement: "验证 PlatformBootstrapper 调用模块注册方法");

            // Rule 2: 启动引导顺序
            ruleSet.AddRule(
                ruleNumber: 2,
                summary: "启动引导顺序",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Solution);

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 1,
                condition: "Host → Platform → Modules 的启动顺序",
                enforcement: "验证 Program.cs 调用顺序");

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 2,
                condition: "禁止模块直接访问 Host 配置",
                enforcement: "验证模块无对 IConfiguration 的直接引用");

            return ruleSet;
        });

        /// <summary>
        /// ADR-003：命名空间规则
        /// 定义命名空间组织、层次结构等规则
        /// </summary>
        public static ArchitectureRuleSet Adr003 => LazyAdr003.Value;

        private static readonly Lazy<ArchitectureRuleSet> LazyAdr003 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(3);

            // Rule 1: 命名空间层次结构
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "命名空间层次结构",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Solution);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "根命名空间为 Zss.BilliardHall",
                enforcement: "验证所有类型命名空间以 Zss.BilliardHall 开头");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "模块命名空间为 Zss.BilliardHall.Modules.{ModuleName}",
                enforcement: "验证模块类型命名空间格式");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 3,
                condition: "Platform 命名空间为 Zss.BilliardHall.Platform",
                enforcement: "验证 Platform 类型命名空间");

            // Rule 2: 命名空间与文件夹对应
            ruleSet.AddRule(
                ruleNumber: 2,
                summary: "命名空间与文件夹对应",
                severity: RuleSeverity.Constitutional,
                scope: RuleScope.Solution);

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 1,
                condition: "命名空间必须与文件夹结构一致",
                enforcement: "验证类型所在文件路径与命名空间匹配");

            return ruleSet;
        });

        #endregion

        #region Governance ADRs (治理层 ADR-900 ~ ADR-999)

        /// <summary>
        /// ADR-900：架构测试与 CI 治理元规则
        /// 定义架构测试的权威性、执行级别、CI 阻断等规则
        /// </summary>
        public static ArchitectureRuleSet Adr900 => LazyAdr900.Value;

        private static readonly Lazy<ArchitectureRuleSet> LazyAdr900 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(900);

            // Rule 1: 架构裁决权威性
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "架构裁决权威性",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Test);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "ADR 正文是唯一裁决依据",
                enforcement: "验证 ADR 文档存在且包含唯一裁决源声明");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "架构违规的判定原则",
                enforcement: "测试失败、CI Gate 失败、人工否决或破例过期均构成违规");

            // Rule 2: 执行级别与测试映射
            ruleSet.AddRule(
                ruleNumber: 2,
                summary: "执行级别与测试映射",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Test);

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 1,
                condition: "执行级别分离原则",
                enforcement: "所有规则必须归类为 L1/L2/L3");

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 2,
                condition: "ADR ↔ 测试 ↔ CI 的一一映射",
                enforcement: "每个 L1 规则必须有对应的架构测试");

            // Rule 3: 破例治理机制
            ruleSet.AddRule(
                ruleNumber: 3,
                summary: "破例治理机制",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Document);

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 1,
                condition: "破例强制要求",
                enforcement: "所有破例必须通过 Issue 记录并设置到期时间");

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 2,
                condition: "CI 自动监控机制",
                enforcement: "CI 检查破例是否过期");

            // Rule 4: 冲突裁决优先级
            ruleSet.AddRule(
                ruleNumber: 4,
                summary: "冲突裁决优先级",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Document);

            ruleSet.AddClause(
                ruleNumber: 4,
                clauseNumber: 1,
                condition: "裁决优先级顺序",
                enforcement: "宪法层 > 治理层 > 技术层，新 ADR > 旧 ADR");

            return ruleSet;
        });

        /// <summary>
        /// ADR-907：ArchitectureTests 执法治理体系
        /// 定义架构测试的命名、组织、断言等规则
        /// </summary>
        public static ArchitectureRuleSet Adr907 => LazyAdr907.Value;

        private static readonly Lazy<ArchitectureRuleSet> LazyAdr907 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(907);

            // Rule 1: 测试文件组织规范
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "测试文件组织规范",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Test);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "测试文件按 ADR 编号组织",
                enforcement: "验证测试文件位于 ADR_{编号} 目录下");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "测试类命名格式 ADR_{编号}_{Rule}_Architecture_Tests",
                enforcement: "验证测试类名称格式");

            // Rule 2: 测试方法命名规范
            ruleSet.AddRule(
                ruleNumber: 2,
                summary: "测试方法命名规范",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Test);

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 1,
                condition: "测试方法名称包含 RuleId",
                enforcement: "验证方法名以 ADR_{编号}_{Rule}_{Clause}_ 开头");

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 2,
                condition: "DisplayName 包含完整 RuleId",
                enforcement: "验证 DisplayName 特性包含 ADR-{编号}_{Rule}_{Clause}");

            // Rule 3: 最小断言语义规范
            ruleSet.AddRule(
                ruleNumber: 3,
                summary: "最小断言语义规范",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Test);

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 1,
                condition: "每个测试类至少包含1个有效断言",
                enforcement: "通过静态分析验证断言数量");

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 2,
                condition: "每个测试方法只能映射一个ADR子规则",
                enforcement: "通过命名模式检查验证");

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 3,
                condition: "所有断言失败信息必须可反向溯源到ADR",
                enforcement: "验证失败消息包含ADR引用、违规标记、修复建议和文档引用");

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 4,
                condition: "禁止形式化断言",
                enforcement: "禁止 Assert.True(true) 等无意义断言");

            // Rule 4: RuleId 格式规范
            ruleSet.AddRule(
                ruleNumber: 4,
                summary: "RuleId 格式规范",
                severity: RuleSeverity.Governance,
                scope: RuleScope.Document);

            ruleSet.AddClause(
                ruleNumber: 4,
                clauseNumber: 1,
                condition: "RuleId 格式为 ADR-{编号}_{Rule}_{Clause}",
                enforcement: "验证所有 RuleId 使用下划线分隔");

            return ruleSet;
        });

        #endregion

        #region Runtime ADRs (运行时 ADR-201 ~ ADR-240)

        /// <summary>
        /// ADR-201：Handler 生命周期管理
        /// 定义 Handler 的创建、执行、释放等规则
        /// </summary>
        public static ArchitectureRuleSet Adr201 => LazyAdr201.Value;

        private static readonly Lazy<ArchitectureRuleSet> LazyAdr201 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(201);

            // Rule 1: Handler 注册规范
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "Handler 注册规范",
                severity: RuleSeverity.Technical,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "Handler 必须通过 DI 容器注册",
                enforcement: "验证 Handler 类型已注册到 IServiceCollection");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "Handler 生命周期必须为 Scoped",
                enforcement: "验证 Handler 注册为 ServiceLifetime.Scoped");

            return ruleSet;
        });

        #endregion

        #region Structure ADRs (结构层 ADR-120 ~ ADR-124)

        /// <summary>
        /// ADR-120：领域事件命名规范
        /// 定义事件命名、命名空间、内容约束等规则
        /// </summary>
        public static ArchitectureRuleSet Adr120 => LazyAdr120.Value;

        private static readonly Lazy<ArchitectureRuleSet> LazyAdr120 = new(() =>
        {
            var ruleSet = new ArchitectureRuleSet(120);

            // Rule 1: 事件类型命名规范
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: "事件类型命名规范",
                severity: RuleSeverity.Technical,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "事件命名模式强制要求",
                enforcement: "验证事件类名以 Event 后缀结尾且使用动词过去式");

            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 2,
                condition: "事件命名空间组织规范",
                enforcement: "验证事件在 Modules.{ModuleName}.Events 命名空间下");

            // Rule 2: 事件处理器命名规范
            ruleSet.AddRule(
                ruleNumber: 2,
                summary: "事件处理器命名规范",
                severity: RuleSeverity.Technical,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 2,
                clauseNumber: 1,
                condition: "事件处理器命名模式",
                enforcement: "验证处理器以 Handler 后缀结尾");

            // Rule 3: 事件内容约束
            ruleSet.AddRule(
                ruleNumber: 3,
                summary: "事件内容约束",
                severity: RuleSeverity.Technical,
                scope: RuleScope.Module);

            ruleSet.AddClause(
                ruleNumber: 3,
                clauseNumber: 1,
                condition: "事件内容类型约束",
                enforcement: "验证事件不包含领域实体和业务方法");

            return ruleSet;
        });

        #endregion

        #region Helper Methods

        /// <summary>
        /// 获取指定 ADR 的规则集
        /// </summary>
        /// <param name="adrNumber">ADR 编号</param>
        /// <returns>规则集，如果不存在则返回 null</returns>
        public static ArchitectureRuleSet? GetRuleSet(int adrNumber)
        {
            return adrNumber switch
            {
                1 => Adr001,
                2 => Adr002,
                3 => Adr003,
                120 => Adr120,
                201 => Adr201,
                900 => Adr900,
                907 => Adr907,
                _ => null
            };
        }

        /// <summary>
        /// 获取所有已定义的规则集
        /// </summary>
        public static IEnumerable<ArchitectureRuleSet> GetAllRuleSets()
        {
            yield return Adr001;
            yield return Adr002;
            yield return Adr003;
            yield return Adr120;
            yield return Adr201;
            yield return Adr900;
            yield return Adr907;
        }

        /// <summary>
        /// 获取所有已定义的 ADR 编号
        /// </summary>
        public static IEnumerable<int> GetAllAdrNumbers()
        {
            yield return 1;
            yield return 2;
            yield return 3;
            yield return 120;
            yield return 201;
            yield return 900;
            yield return 907;
        }

        #endregion
    }
}
