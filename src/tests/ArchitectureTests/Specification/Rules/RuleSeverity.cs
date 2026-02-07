namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 规则严重程度
/// 直接对齐 ADR 层级，用于确定违规的处理策略
/// </summary>
public enum RuleSeverity
{
    /// <summary>
    /// 宪法级 - 违反即阻断
    /// 对应 ADR-001 ~ ADR-099 宪法层文档
    /// 任何违规都必须立即修复，不允许合并
    /// </summary>
    Constitutional,

    /// <summary>
    /// 治理级 - PR 阻断
    /// 对应 ADR-900+ 治理层文档
    /// 违规会阻止 PR 合并，需要在合并前修复
    /// </summary>
    Governance,

    /// <summary>
    /// 技术级 - 架构警告
    /// 对应技术层和结构层文档
    /// 违规会产生警告，但不强制阻断
    /// </summary>
    Technical
}
