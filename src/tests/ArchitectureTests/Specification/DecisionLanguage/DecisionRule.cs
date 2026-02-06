namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// 裁决规则定义
/// 定义一条裁决语言的识别规则和执行策略
/// </summary>
/// <param name="Level">裁决级别（Must/MustNot/Should）</param>
/// <param name="Keywords">触发该规则的关键词列表</param>
/// <param name="IsBlocking">是否具备阻断权力（CI失败）</param>
public sealed record DecisionRule(
    DecisionLevel Level,
    IReadOnlyList<string> Keywords,
    bool IsBlocking
);
