namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// 裁决解析结果
/// 表示从自然语言文本中解析出的裁决级别
/// 
/// 设计原则：
/// - 专注于"解析"职责：将自然语言转换为裁决级别
/// - 不包含执行策略：不关心是否阻断、如何处理
/// - 与 DecisionExecutionResult 分离：保持单一职责
/// </summary>
/// <param name="Level">解析出的裁决级别（null 表示未识别到裁决语言）</param>
public sealed record DecisionParseResult(
    DecisionLevel? Level)
{
    /// <summary>
    /// 表示"无裁决语言"的默认结果
    /// 当文本中未识别到任何裁决关键词时返回此值
    /// </summary>
    public static readonly DecisionParseResult None = new(Level: null);

    /// <summary>
    /// 判断是否成功解析到裁决语言
    /// </summary>
    public bool HasDecision => Level.HasValue;
}

/// <summary>
/// 裁决执行结果
/// 表示裁决的执行策略和行为
/// 
/// 设计原则：
/// - 专注于"执行"职责：定义如何处理已识别的裁决
/// - 包含执行策略：是否阻断、严重程度等
/// - 从 DecisionParseResult 转换而来
/// </summary>
/// <param name="Level">裁决级别</param>
/// <param name="IsBlocking">是否阻断构建/CI</param>
public sealed record DecisionExecutionResult(
    DecisionLevel Level,
    bool IsBlocking);

/// <summary>
/// 裁决解析结果（向后兼容API）
/// 
/// ⚠️ Legacy API：此为旧版本兼容层
/// 
/// 新代码请使用：
/// - DecisionParseResult：用于解析场景（纯解析，无执行策略）
/// - DecisionExecutionResult：用于执行场景（包含阻断策略）
/// 
/// 保留原因：
/// - 维护向后兼容性
/// - 现有测试依赖此API
/// - 计划在v3.0版本移除
/// 
/// 迁移路径：
/// 1. 解析场景：使用 DecisionParseResult
/// 2. 执行场景：先Parse得到Level，再构造ExecutionResult
/// 3. 测试代码：逐步迁移到新API
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
