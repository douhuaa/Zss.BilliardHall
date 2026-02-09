namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 架构规则的层级
/// 显式定义裁决粒度，未来可用于策略分流
/// </summary>
public enum RuleLevel
{
    /// <summary>
    /// 规则层级 - ADR-XXXX.Y 格式
    /// 表示一个完整的架构规则
    /// </summary>
    Rule,

    /// <summary>
    /// 条款层级 - ADR-XXXX.Y.Z 格式
    /// 表示规则的具体执行条件
    /// </summary>
    Clause
}
