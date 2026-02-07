namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

/// <summary>
/// 裁决规则定义
/// 定义一条裁决语言的识别规则和执行策略
/// </summary>
/// <param name="Level">裁决级别（Must/MustNot/Should）</param>
/// <param name="Keywords">触发该规则的关键词列表</param>
public sealed record DecisionRule(
    DecisionLevel Level,
    IReadOnlyList<string> Keywords)
{
    /// <summary>
    /// 是否具备阻断权力（CI失败）
    /// 根据 ADR-905 规范自动计算：
    /// - Must / MustNot → 阻断（true）
    /// - Should → 非阻断（false）
    /// </summary>
    public bool IsBlocking => Level switch
    {
        DecisionLevel.Must => true,
        DecisionLevel.MustNot => true,
        DecisionLevel.Should => false,
        _ => false
    };
};
