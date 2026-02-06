namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// 语义规范定义
/// 定义文档和代码中的关键语义元素，用于裁决和验证
/// </summary>
public sealed class _Semantics
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static readonly _Semantics Instance = new();

    /// <summary>
    /// 裁决性关键词列表
    /// 这些词不应该出现在非裁决性文档（如 Onboarding）中作为新规则定义
    /// </summary>
    public IReadOnlyList<string> DecisionKeywords { get; } =
    [
        "必须", "禁止", "不得", "强制", "不允许"
    ];

    /// <summary>
    /// 关键语义块标题（必须是 ## 级别且唯一）
    /// ADR 文档中的核心结构性标题
    /// </summary>
    public IReadOnlyList<string> RequiredHeadings { get; } =
    [
        "Relationships",
        "Decision",
        "Enforcement",
        "Glossary"
    ];

    private _Semantics() { }
}
