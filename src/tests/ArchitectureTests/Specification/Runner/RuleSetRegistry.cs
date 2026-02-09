using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Domains;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Runner;

/// <summary>
/// 集中化规则集注册表
/// 提供所有领域规则集的统一访问入口，支持新的 RS 编号系统
/// 与现有的 Index.RuleSetRegistry（基于 ADR）并存，互不冲突
/// </summary>
public static class CentralizedRuleSetRegistry
{
    /// <summary>
    /// 获取所有已注册的规则
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>所有规则的集合</returns>
    public static IEnumerable<RuleDefinition> All(ArchitectureRulesOptions options)
    {
        // 命名规则（RS-001 ~ RS-003）
        foreach (var rule in NamingRuleset.GetRules(options))
        {
            yield return rule;
        }

        // 领域事件规则（RS-010 ~ RS-013）
        foreach (var rule in DomainEventRuleset.GetRules())
        {
            yield return rule;
        }

        // 仓储规则（RS-020 ~ RS-023）
        foreach (var rule in RepositoryRuleset.GetRules())
        {
            yield return rule;
        }

        // 反作弊规则（RS-030 ~ RS-032）
        foreach (var rule in AntiCheatRuleset.GetRules(options))
        {
            yield return rule;
        }
    }

    /// <summary>
    /// 按层级获取规则
    /// </summary>
    /// <param name="layer">规则层级</param>
    /// <param name="options">配置选项</param>
    /// <returns>指定层级的规则</returns>
    public static IEnumerable<RuleDefinition> GetByLayer(RuleLayer layer, ArchitectureRulesOptions options)
    {
        return All(options).Where(r => r.Layer == layer);
    }

    /// <summary>
    /// 按严重程度获取规则
    /// </summary>
    /// <param name="severity">严重程度</param>
    /// <param name="options">配置选项</param>
    /// <returns>指定严重程度的规则</returns>
    public static IEnumerable<RuleDefinition> GetBySeverity(SeverityLevel severity, ArchitectureRulesOptions options)
    {
        return All(options).Where(r => r.Severity == severity);
    }

    /// <summary>
    /// 按 ADR 获取规则
    /// </summary>
    /// <param name="adr">ADR 编号（如 "ADR-122"）</param>
    /// <param name="options">配置选项</param>
    /// <returns>来自指定 ADR 的规则</returns>
    public static IEnumerable<RuleDefinition> GetByAdr(string adr, ArchitectureRulesOptions options)
    {
        return All(options).Where(r => r.Id.Adr.Equals(adr, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取治理层规则（Governance）
    /// </summary>
    public static IEnumerable<RuleDefinition> GetGovernanceRules(ArchitectureRulesOptions options)
    {
        return GetByLayer(RuleLayer.Governance, options);
    }

    /// <summary>
    /// 获取执行层规则（Enforcement）
    /// </summary>
    public static IEnumerable<RuleDefinition> GetEnforcementRules(ArchitectureRulesOptions options)
    {
        return GetByLayer(RuleLayer.Enforcement, options);
    }

    /// <summary>
    /// 获取启发层规则（Heuristics）
    /// </summary>
    public static IEnumerable<RuleDefinition> GetHeuristicsRules(ArchitectureRulesOptions options)
    {
        return GetByLayer(RuleLayer.Heuristics, options);
    }

    /// <summary>
    /// 获取 L1 级别规则（阻断 CI）
    /// </summary>
    public static IEnumerable<RuleDefinition> GetL1Rules(ArchitectureRulesOptions options)
    {
        return GetBySeverity(SeverityLevel.L1, options);
    }

    /// <summary>
    /// 获取 L2 级别规则（需人工审查）
    /// </summary>
    public static IEnumerable<RuleDefinition> GetL2Rules(ArchitectureRulesOptions options)
    {
        return GetBySeverity(SeverityLevel.L2, options);
    }

    /// <summary>
    /// 获取 L3 级别规则（仅警告）
    /// </summary>
    public static IEnumerable<RuleDefinition> GetL3Rules(ArchitectureRulesOptions options)
    {
        return GetBySeverity(SeverityLevel.L3, options);
    }

    /// <summary>
    /// 获取规则总数
    /// </summary>
    public static int Count(ArchitectureRulesOptions options)
    {
        return All(options).Count();
    }

    /// <summary>
    /// 获取规则统计信息
    /// </summary>
    public static RuleStatistics GetStatistics(ArchitectureRulesOptions options)
    {
        var all = All(options).ToArray();
        return new RuleStatistics
        {
            Total = all.Length,
            GovernanceCount = all.Count(r => r.Layer == RuleLayer.Governance),
            EnforcementCount = all.Count(r => r.Layer == RuleLayer.Enforcement),
            HeuristicsCount = all.Count(r => r.Layer == RuleLayer.Heuristics),
            L1Count = all.Count(r => r.Severity == SeverityLevel.L1),
            L2Count = all.Count(r => r.Severity == SeverityLevel.L2),
            L3Count = all.Count(r => r.Severity == SeverityLevel.L3)
        };
    }
}

/// <summary>
/// 规则统计信息
/// </summary>
public sealed record RuleStatistics
{
    public int Total { get; init; }
    public int GovernanceCount { get; init; }
    public int EnforcementCount { get; init; }
    public int HeuristicsCount { get; init; }
    public int L1Count { get; init; }
    public int L2Count { get; init; }
    public int L3Count { get; init; }
}
