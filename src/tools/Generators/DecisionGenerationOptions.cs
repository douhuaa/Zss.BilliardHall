namespace Zss.BilliardHall.Generators;

/// <summary>
/// Decision 生成选项
/// </summary>
public sealed record DecisionGenerationOptions
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

    /// <summary>
    /// 验证选项的有效性
    /// </summary>
    public void Validate()
    {
        // Rule 标题是 H3 (3+offset)，Clause 标题是 H4 (4+offset)
        // 为确保所有标题都在 H2-H6 范围内，offset 最大为 2
        if (HeaderLevelOffset is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(
            nameof(HeaderLevelOffset),
            HeaderLevelOffset,
            "HeaderLevelOffset 必须在 0-2 之间，以确保所有标题层级（H3-H6）都不超过 H6");
        }
    }
}
