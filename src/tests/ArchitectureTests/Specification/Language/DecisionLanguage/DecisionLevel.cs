namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

/// <summary>
/// 裁决级别（与 ADR-905 对齐）
/// 定义架构规则的裁决强度和执行方式
/// </summary>
public enum DecisionLevel
{
    /// <summary>
    /// 必须（MUST）
    /// 表示强制性要求，违反将阻断构建/CI
    /// 对应 ADR-905 的 L1 级别执行
    /// </summary>
    Must,

    /// <summary>
    /// 禁止（MUST NOT）
    /// 表示明确禁止的行为，违反将阻断构建/CI
    /// 对应 ADR-905 的 L1 级别执行
    /// </summary>
    MustNot,

    /// <summary>
    /// 应该（SHOULD）
    /// 表示推荐性建议，违反仅产生警告不阻断
    /// 对应 ADR-905 的 L2 级别执行
    /// </summary>
    Should
}
