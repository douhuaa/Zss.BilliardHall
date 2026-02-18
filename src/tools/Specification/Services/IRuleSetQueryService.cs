namespace Zss.BilliardHall.Specification.Services;

/// <summary>
/// 规则集查询服务接口
/// 提供统一的RuleSet查询门面，简化命令处理器对RuleSetRegistry的访问
/// </summary>
public interface IRuleSetQueryService
{
    /// <summary>
    /// 获取规则集（严格模式）
    /// </summary>
    /// <param name="adrNumber">ADR编号</param>
    /// <returns>规则集</returns>
    /// <exception cref="InvalidOperationException">规则集不存在时抛出</exception>
    ArchitectureRuleSet GetRuleSetStrict(int adrNumber);

    /// <summary>
    /// 获取规则集（宽容模式）
    /// </summary>
    /// <param name="adrNumber">ADR编号</param>
    /// <returns>规则集，不存在时返回null</returns>
    ArchitectureRuleSet? GetRuleSet(int adrNumber);

    /// <summary>
    /// 获取所有规则集
    /// </summary>
    IEnumerable<ArchitectureRuleSet> GetAllRuleSets();

    /// <summary>
    /// 按严重程度筛选规则集
    /// </summary>
    IEnumerable<ArchitectureRuleSet> GetRuleSetsBySeverity(RuleSeverity severity);

    /// <summary>
    /// 按作用域筛选规则集
    /// </summary>
    IEnumerable<ArchitectureRuleSet> GetRuleSetsByScope(RuleScope scope);

    /// <summary>
    /// 格式化RuleId为标准字符串格式
    /// </summary>
    /// <param name="ruleId">RuleId对象</param>
    /// <returns>格式化的字符串（如 ADR-001.2 或 ADR-001.2.1）</returns>
    string FormatRuleId(ArchitectureRuleId ruleId);

    /// <summary>
    /// 创建规则集摘要（用于日志和报告）
    /// </summary>
    RuleSetSummary CreateSummary(ArchitectureRuleSet ruleSet);
}

/// <summary>
/// 规则集摘要（用于日志输出）
/// </summary>
public sealed record RuleSetSummary(
    int AdrNumber,
    string FormattedId,
    int RuleCount,
    int ClauseCount,
    IReadOnlyList<RuleSeverity> Severities,
    IReadOnlyList<RuleScope> Scopes)
{
    public override string ToString() =>
        $"{FormattedId}: {RuleCount} rules, {ClauseCount} clauses";
}
