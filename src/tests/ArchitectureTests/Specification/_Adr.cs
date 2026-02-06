namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// ADR 规范定义
/// 定义 ADR 相关的所有规范，包括命名模式、路径约定和已知文档
/// 这是架构治理的核心规范
/// </summary>
public sealed class _Adr
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static readonly _Adr Instance = new();

    /// <summary>
    /// 命名和格式模式定义（通过实例访问）
    /// </summary>
    public PatternsSpec Patterns => PatternsSpec.Instance;

    /// <summary>
    /// ADR 文档路径约定（通过实例访问）
    /// </summary>
    public PathsSpec Paths => PathsSpec.Instance;

    /// <summary>
    /// 已知的关键 ADR 文档路径（通过实例访问）
    /// </summary>
    public KnownDocumentsSpec KnownDocuments => KnownDocumentsSpec.Instance;

    private _Adr()
    {
    }

    /// <summary>
    /// ADR 命名和格式模式
    /// </summary>
    public sealed class PatternsSpec
    {
        public static readonly PatternsSpec Instance = new();
        private PatternsSpec() { }

        /// <summary>
        /// ADR 测试类命名模式（正则表达式）
        /// 匹配格式：ADR_0001_Architecture_Tests
        /// </summary>
        public string TestClass => @"ADR_(\d{4})_Architecture_Tests";

        /// <summary>
        /// ADR 文件命名模式（正则表达式）
        /// 匹配格式：ADR-0001-description.md
        /// </summary>
        public string FileName => @"^ADR-\d{4}[^/\\]*\.md$";

        /// <summary>
        /// ADR 编号模式（正则表达式）
        /// 匹配格式：ADR-0001
        /// </summary>
        public string Id => @"^ADR-\d{4}$";
    }

    /// <summary>
    /// ADR 文档路径约定（相对于仓库根目录）
    /// </summary>
    public sealed class PathsSpec
    {
        public static readonly PathsSpec Instance = new();
        private PathsSpec() { }

        /// <summary>
        /// ADR 文档根目录路径
        /// </summary>
        public string Root => "docs/adr";

        /// <summary>
        /// ADR 宪法层文档路径
        /// </summary>
        public string Constitutional => "docs/adr/constitutional";

        /// <summary>
        /// ADR 治理层文档路径
        /// </summary>
        public string Governance => "docs/adr/governance";

        /// <summary>
        /// ADR 技术层文档路径
        /// </summary>
        public string Technical => "docs/adr/technical";

        /// <summary>
        /// ADR 结构层文档路径
        /// </summary>
        public string Structure => "docs/adr/structure";

        /// <summary>
        /// 案例库路径
        /// </summary>
        public string Cases => "docs/cases";

        /// <summary>
        /// Agent 配置文件路径
        /// </summary>
        public string AgentFiles => ".github/agents";
    }

    /// <summary>
    /// 已知的关键 ADR 文档路径
    /// 这些是系统中经常引用的核心 ADR 文档
    /// </summary>
    public sealed class KnownDocumentsSpec
    {
        public static readonly KnownDocumentsSpec Instance = new();
        private KnownDocumentsSpec() { }

        /// <summary>
        /// ADR-007：Agent 行为与权限宪法
        /// </summary>
        public string Adr007 => "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md";

        /// <summary>
        /// ADR-008：文档治理宪法
        /// </summary>
        public string Adr008 => "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md";

        /// <summary>
        /// ADR-004：中央包管理 (CPM) 规范
        /// </summary>
        public string Adr004 => "docs/adr/constitutional/ADR-004-Cpm-Final.md";

        /// <summary>
        /// ADR-900：架构测试元规则
        /// </summary>
        public string Adr900 => "docs/adr/governance/ADR-900-architecture-testing-meta-rules.md";

        /// <summary>
        /// ADR-901：架构测试反作弊机制
        /// </summary>
        public string Adr901 => "docs/adr/governance/ADR-901-architecture-test-anti-cheating.md";

        /// <summary>
        /// ADR-902：ADR 文档质量规范
        /// </summary>
        public string Adr902 => "docs/adr/governance/ADR-902-adr-template-structure-contract.md";

        /// <summary>
        /// ADR-905：RuleId 格式标准
        /// </summary>
        public string Adr905 => "docs/adr/governance/ADR-905-ruleid-format-standard.md";

        /// <summary>
        /// ADR-907：ArchitectureTests 执法治理体系
        /// </summary>
        public string Adr907 => "docs/adr/governance/ADR-907-architecturetests-enforcement-governance.md";

        /// <summary>
        /// ADR-907-A：ADR-907 对齐执行标准
        /// </summary>
        public string Adr907A => "docs/adr/governance/ADR-907-A-adr907-alignment-execution-standard.md";

        /// <summary>
        /// ADR-910：文档交叉引用规范
        /// </summary>
        public string Adr910 => "docs/adr/governance/ADR-910-documentation-cross-reference-specification.md";

        /// <summary>
        /// ADR-946：ADR 标题级别语义约束
        /// </summary>
        public string Adr946 => "docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md";

        /// <summary>
        /// ADR-951：案例库管理
        /// </summary>
        public string Adr951 => "docs/adr/governance/ADR-951-case-repository-management.md";

        /// <summary>
        /// ADR-960：Onboarding 文档治理
        /// </summary>
        public string Adr960 => "docs/adr/governance/ADR-960-onboarding-documentation-governance.md";

        /// <summary>
        /// ADR-965：Onboarding 互动式学习路径
        /// </summary>
        public string Adr965 => "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md";
    }
}
