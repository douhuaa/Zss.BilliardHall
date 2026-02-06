namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// ADR 规范定义
/// 定义 ADR 相关的所有规范，包括命名模式、路径约定和已知文档
/// 这是架构治理的核心规范
/// </summary>
public static partial class ArchitectureTestSpecification
{
    public static partial class Adr
    {
        /// <summary>
        /// ADR 命名和格式模式
        /// </summary>
        public static class Patterns
        {
            /// <summary>
            /// ADR 测试类命名模式（正则表达式）
            /// 匹配格式：ADR_0001_Architecture_Tests
            /// </summary>
            public static string TestClass => @"ADR_(\d{4})_Architecture_Tests";

            /// <summary>
            /// ADR 文件命名模式（正则表达式）
            /// 匹配格式：ADR-0001-description.md
            /// </summary>
            public static string FileName => @"^ADR-\d{4}[^/\\]*\.md$";

            /// <summary>
            /// ADR 编号模式（正则表达式）
            /// 匹配格式：ADR-0001
            /// </summary>
            public static string Id => @"^ADR-\d{4}$";
        }

        /// <summary>
        /// ADR 文档路径约定（相对于仓库根目录）
        /// </summary>
        public static class Paths
        {
            /// <summary>
            /// ADR 文档根目录路径
            /// </summary>
            public static string Root => "docs/adr";

            /// <summary>
            /// ADR 宪法层文档路径
            /// </summary>
            public static string Constitutional => "docs/adr/constitutional";

            /// <summary>
            /// ADR 治理层文档路径
            /// </summary>
            public static string Governance => "docs/adr/governance";

            /// <summary>
            /// ADR 技术层文档路径
            /// </summary>
            public static string Technical => "docs/adr/technical";

            /// <summary>
            /// ADR 结构层文档路径
            /// </summary>
            public static string Structure => "docs/adr/structure";

            /// <summary>
            /// 案例库路径
            /// </summary>
            public static string Cases => "docs/cases";

            /// <summary>
            /// Agent 配置文件路径
            /// </summary>
            public static string AgentFiles => ".github/agents";
        }

        /// <summary>
        /// 已知的关键 ADR 文档路径
        /// 这些是系统中经常引用的核心 ADR 文档
        /// </summary>
        public static class KnownDocuments
        {
            #region Constitutional ADRs (宪法层)

            /// <summary>
            /// ADR-001：模块化单体与垂直切片架构
            /// </summary>
            public static string Adr001 => "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md";

            /// <summary>
            /// ADR-002：Platform/Application/Host 启动引导
            /// </summary>
            public static string Adr002 => "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md";

            /// <summary>
            /// ADR-003：命名空间规则
            /// </summary>
            public static string Adr003 => "docs/adr/constitutional/ADR-003-namespace-rules.md";

            /// <summary>
            /// ADR-004：中央包管理 (CPM) 规范
            /// </summary>
            public static string Adr004 => "docs/adr/constitutional/ADR-004-Cpm-Final.md";

            /// <summary>
            /// ADR-005：应用交互模型
            /// </summary>
            public static string Adr005 => "docs/adr/constitutional/ADR-005-Application-Interaction-Model-Final.md";

            /// <summary>
            /// ADR-006：术语与编号宪法
            /// </summary>
            public static string Adr006 => "docs/adr/constitutional/ADR-006-terminology-numbering-constitution.md";

            /// <summary>
            /// ADR-007：Agent 行为与权限宪法
            /// </summary>
            public static string Adr007 => "docs/adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md";

            /// <summary>
            /// ADR-008：文档治理宪法
            /// </summary>
            public static string Adr008 => "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md";

            #endregion

            #region Governance ADRs (治理层)

            /// <summary>
            /// ADR-900：架构测试
            /// </summary>
            public static string Adr900 => "docs/adr/governance/ADR-900-architecture-tests.md";

            /// <summary>
            /// ADR-901：警告约束语义
            /// </summary>
            public static string Adr901 => "docs/adr/governance/ADR-901-warning-constraint-semantics.md";

            /// <summary>
            /// ADR-902：ADR 文档质量规范
            /// </summary>
            public static string Adr902 => "docs/adr/governance/ADR-902-adr-template-structure-contract.md";

            /// <summary>
            /// ADR-905：执法级别分类
            /// </summary>
            public static string Adr905 => "docs/adr/governance/ADR-905-enforcement-level-classification.md";

            /// <summary>
            /// ADR-907：ArchitectureTests 执法治理体系
            /// </summary>
            public static string Adr907 => "docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md";

            /// <summary>
            /// ADR-907-A：ADR-907 对齐检查清单
            /// </summary>
            public static string Adr907A => "docs/adr/governance/ADR-907-a-alignment-checklist.md";

            /// <summary>
            /// ADR-910：README 治理宪法
            /// </summary>
            public static string Adr910 => "docs/adr/governance/ADR-910-readme-governance-constitution.md";

            /// <summary>
            /// ADR-920：示例治理宪法
            /// </summary>
            public static string Adr920 => "docs/adr/governance/ADR-920-examples-governance-constitution.md";

            /// <summary>
            /// ADR-930：代码审查合规
            /// </summary>
            public static string Adr930 => "docs/adr/governance/ADR-930-code-review-compliance.md";

            /// <summary>
            /// ADR-940：ADR 关系可追溯性管理
            /// </summary>
            public static string Adr940 => "docs/adr/governance/ADR-940-adr-relationship-traceability-management.md";

            /// <summary>
            /// ADR-945：ADR 时间线演进视图
            /// </summary>
            public static string Adr945 => "docs/adr/governance/ADR-945-adr-timeline-evolution-view.md";

            /// <summary>
            /// ADR-946：ADR 标题级别语义约束
            /// </summary>
            public static string Adr946 => "docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md";

            /// <summary>
            /// ADR-947：关系章节结构解析安全
            /// </summary>
            public static string Adr947 => "docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md";

            /// <summary>
            /// ADR-950：指南与 FAQ 文档治理
            /// </summary>
            public static string Adr950 => "docs/adr/governance/ADR-950-guide-faq-documentation-governance.md";

            /// <summary>
            /// ADR-951：案例库管理
            /// </summary>
            public static string Adr951 => "docs/adr/governance/ADR-951-case-repository-management.md";

            /// <summary>
            /// ADR-952：工程标准 ADR 边界
            /// </summary>
            public static string Adr952 => "docs/adr/governance/ADR-952-engineering-standard-adr-boundary.md";

            /// <summary>
            /// ADR-955：文档搜索与可发现性
            /// </summary>
            public static string Adr955 => "docs/adr/governance/ADR-955-documentation-search-discoverability.md";

            /// <summary>
            /// ADR-960：Onboarding 文档治理
            /// </summary>
            public static string Adr960 => "docs/adr/governance/ADR-960-onboarding-documentation-governance.md";

            /// <summary>
            /// ADR-965：Onboarding 互动式学习路径
            /// </summary>
            public static string Adr965 => "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md";

            /// <summary>
            /// ADR-970：自动化日志集成标准
            /// </summary>
            public static string Adr970 => "docs/adr/governance/ADR-970-automation-log-integration-standard.md";

            /// <summary>
            /// ADR-975：文档质量监控
            /// </summary>
            public static string Adr975 => "docs/adr/governance/ADR-975-documentation-quality-monitoring.md";

            /// <summary>
            /// ADR-980：ADR 生命周期同步
            /// </summary>
            public static string Adr980 => "docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md";

            /// <summary>
            /// ADR-990：文档演进路线图
            /// </summary>
            public static string Adr990 => "docs/adr/governance/ADR-990-documentation-evolution-roadmap.md";

            #endregion

            #region Runtime ADRs (运行时)

            /// <summary>
            /// ADR-201：Handler 生命周期管理
            /// </summary>
            public static string Adr201 => "docs/adr/runtime/ADR-201-handler-lifecycle-management.md";

            /// <summary>
            /// ADR-210：事件版本兼容性
            /// </summary>
            public static string Adr210 => "docs/adr/runtime/ADR-210-event-versioning-compatibility.md";

            /// <summary>
            /// ADR-220：事件总线集成
            /// </summary>
            public static string Adr220 => "docs/adr/runtime/ADR-220-event-bus-integration.md";

            /// <summary>
            /// ADR-240：Handler 异常约束
            /// </summary>
            public static string Adr240 => "docs/adr/runtime/ADR-240-handler-exception-constraints.md";

            #endregion

            #region Structure ADRs (结构层)

            /// <summary>
            /// ADR-120：领域事件命名约定
            /// </summary>
            public static string Adr120 => "docs/adr/structure/ADR-120-domain-event-naming-convention.md";

            /// <summary>
            /// ADR-121：契约 DTO 命名组织
            /// </summary>
            public static string Adr121 => "docs/adr/structure/ADR-121-contract-dto-naming-organization.md";

            /// <summary>
            /// ADR-122：测试组织命名
            /// </summary>
            public static string Adr122 => "docs/adr/structure/ADR-122-test-organization-naming.md";

            /// <summary>
            /// ADR-123：仓储接口分层
            /// </summary>
            public static string Adr123 => "docs/adr/structure/ADR-123-repository-interface-layering.md";

            /// <summary>
            /// ADR-124：端点命名约束
            /// </summary>
            public static string Adr124 => "docs/adr/structure/ADR-124-endpoint-naming-constraints.md";

            #endregion

            #region Technical ADRs (技术层)

            /// <summary>
            /// ADR-301：集成测试自动化
            /// </summary>
            public static string Adr301 => "docs/adr/technical/ADR-301-integration-test-automation.md";

            /// <summary>
            /// ADR-340：结构化日志监控约束
            /// </summary>
            public static string Adr340 => "docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md";

            /// <summary>
            /// ADR-350：日志与可观测性标准
            /// </summary>
            public static string Adr350 => "docs/adr/technical/ADR-350-logging-observability-standards.md";

            /// <summary>
            /// ADR-360：CI/CD 流水线标准化
            /// </summary>
            public static string Adr360 => "docs/adr/technical/ADR-360-cicd-pipeline-standardization.md";

            #endregion
        }
    }
}
