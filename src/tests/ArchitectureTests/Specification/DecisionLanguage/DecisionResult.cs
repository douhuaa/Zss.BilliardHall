namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// 裁决解析结果
/// 包含解析后的裁决级别和是否具备阻断权力
/// </summary>
/// <param name="Level">解析出的裁决级别</param>
/// <param name="IsBlocking">是否阻断构建/CI</param>
public sealed record DecisionResult(
    DecisionLevel? Level,
    bool IsBlocking
)
{
    /// <summary>
    /// 表示"无裁决语言"的默认结果
    /// 当文本中未识别到任何裁决关键词时返回此值
    /// Level 为 null 表示未识别到裁决语言
    /// </summary>
    public static readonly DecisionResult None = new(
        Level: null,
        IsBlocking: false
    );

    /// <summary>
    /// 判断是否为有效的裁决结果（非 None）
    /// </summary>
    public bool IsDecision => Level.HasValue;
}
