namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.RuleIdLanguage;

/// <summary>
/// 架构规则ID的强类型表示
/// 
/// 这是整个治理体系的最小不可再分单元。
/// 通过类型系统消除以下不确定性：
/// 1. ADR-907.3 到底是 Rule 还是 Clause？
/// 2. 907.3 和 907.03 是否等价？
/// 3. 907.3 是否真的存在？
/// 
/// 特性：
/// - 不可变结构体（record struct）
/// - 类型安全的构造
/// - 规范的字符串格式（ADR-0907.3 或 ADR-0907.3.2）
/// - 可比较、可排序
/// - 可用于测试失败信息
/// - 可映射到文档路径
/// </summary>
public readonly record struct ArchitectureRuleId
    : IComparable<ArchitectureRuleId>
{
    /// <summary>
    /// ADR 编号（如 907）
    /// </summary>
    public int AdrNumber { get; }

    /// <summary>
    /// 规则编号（如 3）
    /// </summary>
    public int RuleNumber { get; }

    /// <summary>
    /// 条款编号（可选，如 2）
    /// 如果为 null，表示这是一个 Rule 级别的ID
    /// </summary>
    public int? ClauseNumber { get; }

    /// <summary>
    /// 获取规则层级
    /// </summary>
    public RuleLevel Level =>
        ClauseNumber is null ? RuleLevel.Rule : RuleLevel.Clause;

    /// <summary>
    /// 判断是否为 Rule 级别
    /// </summary>
    public bool IsRule => ClauseNumber is null;

    /// <summary>
    /// 判断是否为 Clause 级别
    /// </summary>
    public bool IsClause => ClauseNumber is not null;

    /// <summary>
    /// 私有构造函数，确保只能通过工厂方法创建
    /// </summary>
    private ArchitectureRuleId(int adr, int rule, int? clause)
    {
        AdrNumber = adr;
        RuleNumber = rule;
        ClauseNumber = clause;
    }

    /// <summary>
    /// 创建一个 Rule 级别的规则ID
    /// </summary>
    /// <param name="adr">ADR 编号</param>
    /// <param name="rule">规则编号</param>
    /// <returns>Rule 级别的规则ID</returns>
    public static ArchitectureRuleId Rule(int adr, int rule)
        => new(adr, rule, null);

    /// <summary>
    /// 创建一个 Clause 级别的规则ID
    /// </summary>
    /// <param name="adr">ADR 编号</param>
    /// <param name="rule">规则编号</param>
    /// <param name="clause">条款编号</param>
    /// <returns>Clause 级别的规则ID</returns>
    public static ArchitectureRuleId Clause(int adr, int rule, int clause)
        => new(adr, rule, clause);

    /// <summary>
    /// 转换为规范的字符串格式
    /// Rule: ADR-0907.3
    /// Clause: ADR-0907.3.2
    /// </summary>
    public override string ToString()
        => ClauseNumber is null
            ? $"ADR-{AdrNumber:0000}.{RuleNumber}"
            : $"ADR-{AdrNumber:0000}.{RuleNumber}.{ClauseNumber}";

    /// <summary>
    /// 实现可比较接口，用于排序
    /// 排序规则：先按 ADR 编号，再按 Rule 编号，最后按 Clause 编号
    /// 
    /// 重要说明：
    /// - Rule (ClauseNumber=null) 会被视为 ClauseNumber=0
    /// - 这意味着 Rule 总是排在同编号的 Clause 之前
    /// - 例如：ADR-907.3 (Rule) 排在 ADR-907.3.1 (Clause) 之前
    /// </summary>
    public int CompareTo(ArchitectureRuleId other)
    {
        var adr = AdrNumber.CompareTo(other.AdrNumber);
        if (adr != 0) return adr;

        var rule = RuleNumber.CompareTo(other.RuleNumber);
        if (rule != 0) return rule;

        return (ClauseNumber ?? 0).CompareTo(other.ClauseNumber ?? 0);
    }
}
