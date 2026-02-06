namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// Architecture Tests 的规范定义入口
/// 这是一个"规则模型"，不是工具类
/// 
/// 设计目标：
/// 1. 成为 ArchitectureTests 的唯一规范源（命名规则、ADR 世界观、裁决关键词、三态输出标准、语义块定义）
/// 2. 与 TestEnvironment 彻底解耦（不访问文件系统、不依赖运行目录、不关心仓库位置）
/// 3. 为未来 Analyzer/Generator/Agent 做准备（可复用到 Roslyn Analyzer、Source Generator、Copilot Agent 规则镜像）
/// 
/// 组织原则：按"语义域"分组，而不是按"字符串类型"分组
/// </summary>
public static partial class ArchitectureTestSpecification
{
    /// <summary>
    /// 命名空间规范
    /// </summary>
    public static partial class Namespaces
    {
        /// <summary>
        /// 架构测试的命名空间前缀
        /// </summary>
        public static string ArchitectureTests => "Zss.BilliardHall.Tests.ArchitectureTests";

        /// <summary>
        /// ADR 架构测试的命名空间
        /// </summary>
        public static string AdrTests => "Zss.BilliardHall.Tests.ArchitectureTests.ADR";

        /// <summary>
        /// 模块命名空间前缀
        /// </summary>
        public static string Modules => "Zss.BilliardHall.Modules";

        /// <summary>
        /// Platform 命名空间
        /// </summary>
        public static string Platform => "Zss.BilliardHall.Platform";

        /// <summary>
        /// BuildingBlocks 命名空间
        /// </summary>
        public static string BuildingBlocks => "Zss.BilliardHall.BuildingBlocks";

        /// <summary>
        /// Host 命名空间前缀
        /// </summary>
        public static string Host => "Zss.BilliardHall.Host";

        /// <summary>
        /// Platform Bootstrapper 类型全名
        /// </summary>
        public static string PlatformBootstrapperType => "Zss.BilliardHall.Platform.PlatformBootstrapper";
    }

    /// <summary>
    /// ADR 规范（包含 Patterns、Paths、KnownDocuments）
    /// </summary>
    public static partial class Adr
    {
        // Nested static classes defined in _Adr.cs
    }

    /// <summary>
    /// 语义规范（包含裁决关键词、关键标题等）
    /// </summary>
    public static partial class Semantics
    {
        /// <summary>
        /// 裁决性关键词列表
        /// 这些词不应该出现在非裁决性文档（如 Onboarding）中作为新规则定义
        /// </summary>
        public static IReadOnlyList<string> DecisionKeywords { get; } =
        [
            "必须", "禁止", "不得", "强制", "不允许"
        ];

        /// <summary>
        /// 关键语义块标题（必须是 ## 级别且唯一）
        /// ADR 文档中的核心结构性标题
        /// </summary>
        public static IReadOnlyList<string> RequiredHeadings { get; } =
        [
            "Relationships",
            "Decision",
            "Enforcement",
            "Glossary"
        ];
    }

    /// <summary>
    /// 输出规范（包含三态输出标准）
    /// </summary>
    public static partial class Output
    {
        // Nested static class defined in _Output.cs
    }

    /// <summary>
    /// Onboarding 内容规范
    /// </summary>
    public static partial class Onboarding
    {
        /// <summary>
        /// Onboarding 文档禁止的内容类型（核心类型）
        /// </summary>
        public static IReadOnlyList<string> ProhibitedContent =>
        [
            "架构约束定义"
        ];

        /// <summary>
        /// Onboarding 文档允许的内容类型
        /// </summary>
        public static IReadOnlyList<string> AllowedContent =>
        [
            "导航指引",
            "学习路径",
            "示例代码",
            "快速入门"
        ];

        /// <summary>
        /// Onboarding 文档的三个核心问题
        /// </summary>
        public static IReadOnlyList<string> CoreQuestions =>
        [
            "我是谁",
            "我先看什么",
            "我下一步去哪"
        ];
    }
}
