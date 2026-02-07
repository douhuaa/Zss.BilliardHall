namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

/// <summary>
/// ADR 规则索引
/// 提供从 ADR 编号到规则定义的快速索引能力
/// 
/// 主要用途：
/// 1. 快速查找特定 ADR 的特定 Rule 或 Clause
/// 2. 验证 RuleId 的有效性
/// 3. 为测试方法提供规则元数据访问
/// </summary>
public static class AdrRuleIndex
{
    /// <summary>
    /// 根据 RuleId 获取规则定义
    /// </summary>
    /// <param name="ruleId">规则标识（格式：ADR-{编号}_{Rule} 或 ADR-{编号}.{Rule}）</param>
    /// <returns>规则定义，如果不存在则返回 null</returns>
    public static ArchitectureRuleDefinition? GetRule(string ruleId)
    {
        var (adr, rule, clause) = ParseRuleId(ruleId);
        if (adr == null || rule == null || clause != null)
        {
            return null; // 这是条款 ID 或无效 ID，不是规则 ID
        }

        var ruleSet = RuleSetRegistry.Get(adr.Value);
        return ruleSet?.GetRule(rule.Value);
    }

    /// <summary>
    /// 根据 RuleId 获取条款定义
    /// </summary>
    /// <param name="ruleId">规则标识（格式：ADR-{编号}_{Rule}_{Clause} 或 ADR-{编号}.{Rule}.{Clause}）</param>
    /// <returns>条款定义，如果不存在则返回 null</returns>
    public static ArchitectureClauseDefinition? GetClause(string ruleId)
    {
        var (adr, rule, clause) = ParseRuleId(ruleId);
        if (adr == null || rule == null || clause == null)
        {
            return null; // 这是规则 ID 或无效 ID，不是条款 ID
        }

        var ruleSet = RuleSetRegistry.Get(adr.Value);
        return ruleSet?.GetClause(rule.Value, clause.Value);
    }

    /// <summary>
    /// 检查规则是否存在
    /// </summary>
    /// <param name="ruleId">规则标识</param>
    /// <returns>true 如果规则存在</returns>
    public static bool RuleExists(string ruleId)
    {
        return GetRule(ruleId) != null;
    }

    /// <summary>
    /// 检查条款是否存在
    /// </summary>
    /// <param name="ruleId">规则标识</param>
    /// <returns>true 如果条款存在</returns>
    public static bool ClauseExists(string ruleId)
    {
        return GetClause(ruleId) != null;
    }

    /// <summary>
    /// 获取指定 ADR 的所有规则
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>规则列表</returns>
    public static IReadOnlyCollection<ArchitectureRuleDefinition> GetRulesByAdr(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        return ruleSet?.Rules ?? Array.Empty<ArchitectureRuleDefinition>();
    }

    /// <summary>
    /// 获取指定 ADR 的所有条款
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>条款列表</returns>
    public static IReadOnlyCollection<ArchitectureClauseDefinition> GetClausesByAdr(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        return ruleSet?.Clauses ?? Array.Empty<ArchitectureClauseDefinition>();
    }

    /// <summary>
    /// 获取指定 Rule 的所有条款
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <param name="ruleNumber">规则编号</param>
    /// <returns>条款列表</returns>
    public static IReadOnlyCollection<ArchitectureClauseDefinition> GetClausesByRule(
        int adrNumber,
        int ruleNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        if (ruleSet == null)
        {
            return Array.Empty<ArchitectureClauseDefinition>();
        }

        return ruleSet.Clauses
            .Where(c => c.Id.RuleNumber == ruleNumber)
            .ToArray();
    }

    /// <summary>
    /// 验证 RuleId 格式是否正确
    /// </summary>
    /// <param name="ruleId">规则标识</param>
    /// <returns>true 如果格式正确</returns>
    public static bool IsValidRuleId(string ruleId)
    {
        var (adr, rule, _) = ParseRuleId(ruleId);
        return adr != null && rule != null;
    }

    /// <summary>
    /// 解析 RuleId 字符串
    /// 支持格式：
    /// - ADR-{编号}_{Rule}
    /// - ADR-{编号}_{Rule}_{Clause}
    /// - ADR-{编号}.{Rule}
    /// - ADR-{编号}.{Rule}.{Clause}
    /// </summary>
    /// <param name="ruleId">规则标识字符串</param>
    /// <returns>元组 (ADR编号, Rule编号, Clause编号)</returns>
    private static (int? Adr, int? Rule, int? Clause) ParseRuleId(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            return (null, null, null);
        }

        // 移除 "ADR-" 或 "ADR" 前缀
        var normalized = ruleId
            .Replace("ADR-", "", StringComparison.OrdinalIgnoreCase)
            .Replace("ADR", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        // 尝试用下划线分隔
        var underscoreParts = normalized.Split('_');
        if (underscoreParts.Length >= 2)
        {
            if (int.TryParse(underscoreParts[0], out var adr) &&
                int.TryParse(underscoreParts[1], out var rule))
            {
                int? clause = null;
                if (underscoreParts.Length >= 3 && int.TryParse(underscoreParts[2], out var c))
                {
                    clause = c;
                }
                return (adr, rule, clause);
            }
        }

        // 尝试用点号分隔（兼容旧格式）
        var dotParts = normalized.Split('.');
        if (dotParts.Length >= 2)
        {
            if (int.TryParse(dotParts[0], out var adr) &&
                int.TryParse(dotParts[1], out var rule))
            {
                int? clause = null;
                if (dotParts.Length >= 3 && int.TryParse(dotParts[2], out var c))
                {
                    clause = c;
                }
                return (adr, rule, clause);
            }
        }

        return (null, null, null);
    }
}
