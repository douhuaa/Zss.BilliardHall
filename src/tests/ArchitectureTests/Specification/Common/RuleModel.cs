namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

/// <summary>
/// 规则层级：对齐 ADR-900 的三层治理理念
/// </summary>
public enum RuleLayer
{
    /// <summary>
    /// 治理层（Governance）：定义"制度存在"的规则，通常对应宪法层 ADR
    /// </summary>
    Governance,

    /// <summary>
    /// 执行层（Enforcement）：定义"如何执行"的规则，对应技术约束和架构规范
    /// </summary>
    Enforcement,

    /// <summary>
    /// 启发层（Heuristics）：定义"最佳实践建议"，仅警告不阻断
    /// </summary>
    Heuristics
}

/// <summary>
/// 严重程度：对齐 ADR-907 的执行级别分类
/// </summary>
public enum SeverityLevel
{
    /// <summary>
    /// L1：架构测试失败 → 自动阻断 CI
    /// </summary>
    L1,

    /// <summary>
    /// L2：Analyzer 告警 → 需人工 Code Review
    /// </summary>
    L2,

    /// <summary>
    /// L3：启发式建议 → 仅警告，不阻断
    /// </summary>
    L3
}

/// <summary>
/// 规则标识：新编号系统（RS-###）+ ADR 映射
/// </summary>
/// <param name="NewCode">新规则编码（如 RS-001）</param>
/// <param name="Adr">原 ADR 编号（如 ADR-122）</param>
/// <param name="Section">ADR 章节（如 Rule_1_Clause_2）</param>
public sealed record RuleId(string NewCode, string Adr, string Section)
{
    /// <summary>
    /// 格式化为可读字符串
    /// </summary>
    public override string ToString() => $"{NewCode} ({Adr}.{Section})";
}

/// <summary>
/// 规则定义：包含规则的所有元数据和执行逻辑
/// </summary>
/// <param name="Id">规则标识</param>
/// <param name="Title">规则标题</param>
/// <param name="Layer">规则层级</param>
/// <param name="Severity">严重程度</param>
/// <param name="Evaluate">规则验证函数</param>
public sealed record RuleDefinition(
    RuleId Id,
    string Title,
    RuleLayer Layer,
    SeverityLevel Severity,
    Func<Assembly[], RuleResult> Evaluate);

/// <summary>
/// 规则执行结果
/// </summary>
/// <param name="Passed">是否通过</param>
/// <param name="Message">结果消息</param>
/// <param name="Warnings">警告信息（仅用于 Heuristics 层）</param>
public sealed record RuleResult(bool Passed, string Message = "", string[]? Warnings = null)
{
    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static RuleResult Ok(string msg = "") => new(true, msg);

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static RuleResult Fail(string msg) => new(false, msg);

    /// <summary>
    /// 创建仅警告结果（通过但有警告）
    /// </summary>
    public static RuleResult Warning(params string[] warnings) => new(true, "", warnings);

    /// <summary>
    /// 是否仅为警告
    /// </summary>
    public bool IsWarningOnly => Passed && Warnings is { Length: > 0 };
}
