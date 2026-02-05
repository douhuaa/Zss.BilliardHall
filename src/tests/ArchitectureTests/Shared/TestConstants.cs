namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 测试常量定义
/// 集中管理测试中使用的魔法字符串和常量，避免散布在各个测试文件中
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// 架构测试的命名空间前缀
    /// </summary>
    public const string ArchitectureTestNamespace = "Zss.BilliardHall.Tests.ArchitectureTests";

    /// <summary>
    /// ADR 架构测试的命名空间
    /// </summary>
    public const string AdrTestNamespace = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";

    /// <summary>
    /// ADR 测试文件名模式（正则表达式）
    /// 匹配格式：ADR_001_Architecture_Tests
    /// </summary>
    public const string AdrTestPattern = @"ADR_(\d{4})_Architecture_Tests";

    /// <summary>
    /// 模块命名空间前缀
    /// </summary>
    public const string ModuleNamespace = "Zss.BilliardHall.Modules";

    /// <summary>
    /// Platform 命名空间
    /// </summary>
    public const string PlatformNamespace = "Zss.BilliardHall.Platform";

    /// <summary>
    /// Platform Bootstrapper 类型全名
    /// </summary>
    public const string PlatformBootstrapperTypeName = "Zss.BilliardHall.Platform.PlatformBootstrapper";

    /// <summary>
    /// BuildingBlocks 命名空间
    /// </summary>
    public const string BuildingBlocksNamespace = "Zss.BilliardHall.BuildingBlocks";

    /// <summary>
    /// Host 命名空间前缀
    /// </summary>
    public const string HostNamespace = "Zss.BilliardHall.Host";

    /// <summary>
    /// ADR 文件命名模式（正则表达式）
    /// 匹配格式：ADR-001-description.md
    /// </summary>
    public const string AdrFilePattern = @"^ADR-\d{4}[^/\\]*\.md$";

    /// <summary>
    /// ADR 编号模式（正则表达式）
    /// 匹配格式：ADR-001
    /// </summary>
    public const string AdrIdPattern = @"^ADR-\d{4}$";

    /// <summary>
    /// 构建配置（Debug/Release）
    /// </summary>
    public static string BuildConfiguration => 
        Environment.GetEnvironmentVariable("Configuration") ?? "Debug";

    /// <summary>
    /// 支持的目标框架（按优先级排序）
    /// </summary>
    public static readonly string[] SupportedTargetFrameworks = 
    {
        "net10.0",
        "net8.0",
        "net7.0",
        "net6.0",
        "net5.0"
    };

    #region ADR 文档路径常量

    /// <summary>
    /// ADR 文档根目录路径（相对于仓库根目录）
    /// </summary>
    public const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR 宪法层文档路径（相对于仓库根目录）
    /// </summary>
    public const string AdrConstitutionalPath = "docs/adr/constitutional";

    /// <summary>
    /// ADR 治理层文档路径（相对于仓库根目录）
    /// </summary>
    public const string AdrGovernancePath = "docs/adr/governance";

    /// <summary>
    /// ADR 技术层文档路径（相对于仓库根目录）
    /// </summary>
    public const string AdrTechnicalPath = "docs/adr/technical";

    /// <summary>
    /// ADR 结构层文档路径（相对于仓库根目录）
    /// </summary>
    public const string AdrStructurePath = "docs/adr/structure";

    /// <summary>
    /// 案例库路径（相对于仓库根目录）
    /// </summary>
    public const string CasesPath = "docs/cases";

    /// <summary>
    /// Agent 配置文件路径（相对于仓库根目录）
    /// </summary>
    public const string AgentFilesPath = ".github/agents";

    #endregion

    #region 常用 ADR 文档路径

    /// <summary>
    /// ADR-007：Agent 行为与权限宪法文档路径
    /// </summary>
    public const string Adr007Path = "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md";

    /// <summary>
    /// ADR-008：文档治理宪法文档路径
    /// </summary>
    public const string Adr008Path = "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md";

    /// <summary>
    /// ADR-946：ADR 标题级别语义约束文档路径
    /// </summary>
    public const string Adr946Path = "docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md";

    /// <summary>
    /// ADR-951：案例库管理文档路径
    /// </summary>
    public const string Adr951Path = "docs/adr/governance/ADR-951-case-repository-management.md";

    /// <summary>
    /// ADR-960：Onboarding 文档治理文档路径
    /// </summary>
    public const string Adr960Path = "docs/adr/governance/ADR-960-onboarding-documentation-governance.md";

    /// <summary>
    /// ADR-965：Onboarding 互动式学习路径文档路径
    /// </summary>
    public const string Adr965Path = "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md";

    /// <summary>
    /// ADR-004：中央包管理 (CPM) 规范文档路径
    /// </summary>
    public const string Adr004Path = "docs/adr/constitutional/ADR-004-Cpm-Final.md";

    #endregion

    #region 裁决性关键词

    /// <summary>
    /// 裁决性关键词列表 - 这些词不应该出现在非裁决性文档（如 Onboarding）中作为新规则定义
    /// </summary>
    public static readonly string[] DecisionKeywords = new[]
    {
        "必须", "禁止", "不得", "强制", "不允许"
    };

    #endregion

    #region 语义块标题

    /// <summary>
    /// 关键语义块标题（必须是 ## 级别且唯一）
    /// </summary>
    public static readonly string[] KeySemanticHeadings = new[]
    {
        "Relationships",
        "Decision",
        "Enforcement",
        "Glossary"
    };

    #endregion
}
