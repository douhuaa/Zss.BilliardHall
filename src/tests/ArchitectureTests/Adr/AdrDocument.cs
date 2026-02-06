namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR 文档的强类型模型
/// 表示一个 ADR 文档及其关系声明
/// </summary>
public sealed class AdrDocument
{
    /// <summary>
    /// ADR 编号（如 "ADR-001"）
    /// </summary>
    public string Id { get; init; } = default!;

    /// <summary>
    /// 文件的完整路径
    /// </summary>
    public string FilePath { get; init; } = default!;

    /// <summary>
    /// Front Matter 中的 adr 字段值
    /// </summary>
    public string? AdrField { get; init; }

    /// <summary>
    /// 文档类型（从 Front Matter 的 type 字段或推断）
    /// 可能的值：adr, checklist, guide, template, etc.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// ADR 状态（从 Front Matter）
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// 架构层级（从 Front Matter）
    /// </summary>
    public string? Level { get; init; }

    /// <summary>
    /// 是否是正式的 ADR 文档
    /// 基于 Front Matter 和文件名判断
    /// </summary>
    public bool IsAdr { get; init; }

    /// <summary>
    /// 是否有有效的 Front Matter
    /// </summary>
    public bool HasFrontMatter { get; init; }

    /// <summary>
    /// 依赖的 ADR 列表（Depends On）
    /// </summary>
    public HashSet<string> DependsOn { get; } = new();

    /// <summary>
    /// 被哪些 ADR 依赖（Depended By）
    /// </summary>
    public HashSet<string> DependedBy { get; } = new();

    /// <summary>
    /// 替代的 ADR 列表（Supersedes）
    /// </summary>
    public HashSet<string> Supersedes { get; } = new();

    /// <summary>
    /// 被哪些 ADR 替代（Superseded By）
    /// </summary>
    public HashSet<string> SupersededBy { get; } = new();

    /// <summary>
    /// 相关的 ADR 列表（Related）
    /// </summary>
    public HashSet<string> Related { get; } = new();
}
