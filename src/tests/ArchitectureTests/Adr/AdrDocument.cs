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
