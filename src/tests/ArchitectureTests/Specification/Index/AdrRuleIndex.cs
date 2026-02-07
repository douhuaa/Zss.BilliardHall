namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

/// <summary>
/// ADR 规则索引
/// 提供从 RuleId 到规则定义的快速索引能力
/// 
/// 职责边界：
/// - 回答"规则在哪里？"（基于 RuleId 对象）
/// - 不负责 RuleId 字符串解析（由 RuleIdParser 负责）
/// - 不负责 RuleId 语义解释（由 RuleIdParser 负责）
/// 
/// 主要用途：
/// 1. 快速查找特定 RuleId 对应的 Rule 或 Clause 定义
/// 2. 验证 RuleId 的存在性
/// 3. 为测试方法提供规则元数据访问
/// </summary>
public static class AdrRuleIndex
{
    /// <summary>
    /// 根据 RuleId 对象获取规则定义
    /// </summary>
    /// <param name="ruleId">规则标识对象</param>
    /// <returns>规则定义，如果不存在则返回 null</returns>
    public static ArchitectureRuleDefinition? GetRule(ArchitectureRuleId ruleId)
    {
        // 只接受 Rule 级别的 RuleId
        if (!ruleId.IsRule)
        {
            return null;
        }

        var ruleSet = RuleSetRegistry.Get(ruleId.AdrNumber);
        return ruleSet?.GetRule(ruleId.RuleNumber);
    }

    /// <summary>
    /// 根据 RuleId 字符串获取规则定义
    /// </summary>
    /// <param name="ruleId">规则标识字符串（格式：ADR-{编号}_{Rule} 或 ADR-{编号}.{Rule}）</param>
    /// <returns>规则定义，如果不存在或格式错误则返回 null</returns>
    public static ArchitectureRuleDefinition? GetRule(string ruleId)
    {
        if (!RuleIdParser.TryParse(ruleId, out var parsedId))
        {
            return null;
        }

        return GetRule(parsedId);
    }

    /// <summary>
    /// 根据 RuleId 对象获取条款定义
    /// </summary>
    /// <param name="ruleId">规则标识对象</param>
    /// <returns>条款定义，如果不存在则返回 null</returns>
    public static ArchitectureClauseDefinition? GetClause(ArchitectureRuleId ruleId)
    {
        // 只接受 Clause 级别的 RuleId
        if (!ruleId.IsClause)
        {
            return null;
        }

        var ruleSet = RuleSetRegistry.Get(ruleId.AdrNumber);
        return ruleSet?.GetClause(ruleId.RuleNumber, ruleId.ClauseNumber!.Value);
    }

    /// <summary>
    /// 根据 RuleId 字符串获取条款定义
    /// </summary>
    /// <param name="ruleId">规则标识字符串（格式：ADR-{编号}_{Rule}_{Clause} 或 ADR-{编号}.{Rule}.{Clause}）</param>
    /// <returns>条款定义，如果不存在或格式错误则返回 null</returns>
    public static ArchitectureClauseDefinition? GetClause(string ruleId)
    {
        if (!RuleIdParser.TryParse(ruleId, out var parsedId))
        {
            return null;
        }

        return GetClause(parsedId);
    }

    /// <summary>
    /// 检查规则是否存在（基于 RuleId 对象）
    /// </summary>
    /// <param name="ruleId">规则标识对象</param>
    /// <returns>true 如果规则存在</returns>
    public static bool RuleExists(ArchitectureRuleId ruleId)
    {
        return GetRule(ruleId) != null;
    }

    /// <summary>
    /// 检查规则是否存在（基于 RuleId 字符串）
    /// </summary>
    /// <param name="ruleId">规则标识字符串</param>
    /// <returns>true 如果规则存在</returns>
    public static bool RuleExists(string ruleId)
    {
        return GetRule(ruleId) != null;
    }

    /// <summary>
    /// 检查条款是否存在（基于 RuleId 对象）
    /// </summary>
    /// <param name="ruleId">规则标识对象</param>
    /// <returns>true 如果条款存在</returns>
    public static bool ClauseExists(ArchitectureRuleId ruleId)
    {
        return GetClause(ruleId) != null;
    }

    /// <summary>
    /// 检查条款是否存在（基于 RuleId 字符串）
    /// </summary>
    /// <param name="ruleId">规则标识字符串</param>
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
    /// <param name="ruleId">规则标识字符串</param>
    /// <returns>true 如果格式正确</returns>
    public static bool IsValidRuleId(string ruleId)
    {
        return RuleIdParser.IsValidRuleId(ruleId);
    }
}
