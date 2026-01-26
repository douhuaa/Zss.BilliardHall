namespace Zss.BilliardHall.AdrSemanticParser.Models;

/// <summary>
/// ADR 语义模型，表示一个 ADR 文档的完整结构化信息
/// </summary>
public sealed class AdrSemanticModel
{
    /// <summary>
    /// ADR 编号（如 "ADR-0001"）
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// ADR 标题
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// ADR 状态（如 "Final", "Accepted", "Superseded"）
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// ADR 级别（如 "宪法层", "Governance"）
    /// </summary>
    public string? Level { get; init; }

    /// <summary>
    /// 适用范围
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// 生效时间
    /// </summary>
    public string? EffectiveDate { get; init; }

    /// <summary>
    /// 关系声明
    /// </summary>
    public required AdrRelationships Relationships { get; init; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// 决策内容（Markdown 格式）
    /// </summary>
    public string? DecisionContent { get; init; }

    /// <summary>
    /// 术语表
    /// </summary>
    public List<GlossaryEntry> Glossary { get; init; } = [];

    /// <summary>
    /// 快速参考表
    /// </summary>
    public List<QuickReferenceEntry> QuickReference { get; init; } = [];
}

/// <summary>
/// ADR 关系声明
/// </summary>
public sealed class AdrRelationships
{
    /// <summary>
    /// 依赖的 ADR 列表（Depends On）
    /// </summary>
    public List<AdrReference> DependsOn { get; init; } = [];

    /// <summary>
    /// 被依赖的 ADR 列表（Depended By）
    /// </summary>
    public List<AdrReference> DependedBy { get; init; } = [];

    /// <summary>
    /// 替代的 ADR 列表（Supersedes）
    /// </summary>
    public List<AdrReference> Supersedes { get; init; } = [];

    /// <summary>
    /// 被替代的 ADR 列表（Superseded By）
    /// </summary>
    public List<AdrReference> SupersededBy { get; init; } = [];

    /// <summary>
    /// 相关的 ADR 列表（Related）
    /// </summary>
    public List<AdrReference> Related { get; init; } = [];
}

/// <summary>
/// ADR 引用
/// </summary>
public sealed class AdrReference
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// ADR 标题
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// 引用原因或说明
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// 相对路径
    /// </summary>
    public string? RelativePath { get; init; }
}

/// <summary>
/// 术语表条目
/// </summary>
public sealed class GlossaryEntry
{
    /// <summary>
    /// 术语（中文）
    /// </summary>
    public required string Term { get; init; }

    /// <summary>
    /// 定义
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// 英文对照
    /// </summary>
    public string? EnglishTerm { get; init; }
}

/// <summary>
/// 快速参考表条目
/// </summary>
public sealed class QuickReferenceEntry
{
    /// <summary>
    /// 约束编号
    /// </summary>
    public string? ConstraintId { get; init; }

    /// <summary>
    /// 约束描述
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 测试方式
    /// </summary>
    public string? TestMethod { get; init; }

    /// <summary>
    /// 测试用例
    /// </summary>
    public string? TestCase { get; init; }

    /// <summary>
    /// 是否必须遵守
    /// </summary>
    public bool IsMandatory { get; init; }
}
