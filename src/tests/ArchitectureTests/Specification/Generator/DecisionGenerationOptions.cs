namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// Decision 生成选项
/// </summary>
public sealed class DecisionGenerationOptions
{
    /// <summary>
    /// 是否包含章节标题（## Decision）
    /// 默认为 true
    /// </summary>
    public bool IncludeSectionHeader { get; init; } = true;

    /// <summary>
    /// 是否包含警告说明
    /// 默认为 true
    /// </summary>
    public bool IncludeWarningNote { get; init; } = true;

    /// <summary>
    /// 标题层级偏移量
    /// 默认为 0（使用标准层级：## Decision, ### Rule, #### Clause）
    /// 有效范围：0-2（确保 Clause 标题最高为 H6）
    /// </summary>
    public int HeaderLevelOffset { get; init; } = 0;

    /// <summary>
    /// 是否在 Clause 之间添加空行
    /// 默认为 false
    /// </summary>
    public bool AddBlankLinesBetweenClauses { get; init; } = false;

    /// <summary>
    /// 是否对文本进行 Markdown 转义
    /// 默认为 true（推荐保持开启以防止生成破坏性 Markdown）
    /// </summary>
    public bool EscapeMarkdown { get; init; } = true;

    /// <summary>
    /// 默认选项实例
    /// </summary>
    public static DecisionGenerationOptions Default { get; } = new();
}
