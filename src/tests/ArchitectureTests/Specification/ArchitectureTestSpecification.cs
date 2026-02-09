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
        // Nested static classes defined in _Namespaces.cs
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
        // Nested static class defined in _Semantics.cs
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
        // Nested static class defined in _Onboarding.cs
    }
}
