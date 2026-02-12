namespace Zss.BilliardHall.Generators;

/// <summary>
/// 规则集接口
/// 抽象规则数据结构，用于生成器
/// </summary>
public interface IRuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    int AdrNumber { get; }

    /// <summary>
    /// 所有规则
    /// </summary>
    IReadOnlyList<IRule> Rules { get; }

    /// <summary>
    /// 所有条款
    /// </summary>
    IReadOnlyList<IClause> Clauses { get; }
}

/// <summary>
/// 规则接口
/// </summary>
public interface IRule
{
    /// <summary>
    /// 规则 ID
    /// </summary>
    IRuleId Id { get; }

    /// <summary>
    /// 规则摘要
    /// </summary>
    string Summary { get; }
}

/// <summary>
/// 条款接口
/// </summary>
public interface IClause
{
    /// <summary>
    /// 条款 ID
    /// </summary>
    IRuleId Id { get; }

    /// <summary>
    /// 条件描述
    /// </summary>
    string Condition { get; }

    /// <summary>
    /// 执行要求
    /// </summary>
    string Enforcement { get; }
}

/// <summary>
/// 规则 ID 接口
/// </summary>
public interface IRuleId
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    int AdrNumber { get; }

    /// <summary>
    /// 规则编号
    /// </summary>
    int RuleNumber { get; }

    /// <summary>
    /// 条款编号（可选）
    /// </summary>
    int? ClauseNumber { get; }

    /// <summary>
    /// 转换为字符串表示（如 ADR-907_1_2）
    /// </summary>
    string ToString();
}
