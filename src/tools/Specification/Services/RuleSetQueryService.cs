namespace Zss.BilliardHall.Specification.Services;

/// <summary>
/// 规则集查询服务实现
/// 封装RuleSetRegistry的访问，提供统一的查询门面
/// </summary>
public sealed class RuleSetQueryService : IRuleSetQueryService
{
    public ArchitectureRuleSet GetRuleSetStrict(int adrNumber)
    {
        return RuleSetRegistry.GetStrict(adrNumber);
    }

    public ArchitectureRuleSet? GetRuleSet(int adrNumber)
    {
        return RuleSetRegistry.Get(adrNumber);
    }

    public IEnumerable<ArchitectureRuleSet> GetAllRuleSets()
    {
        return RuleSetRegistry.GetAllRuleSets();
    }

    public IEnumerable<ArchitectureRuleSet> GetRuleSetsBySeverity(RuleSeverity severity)
    {
        return RuleSetRegistry.GetBySeverity(severity);
    }

    public IEnumerable<ArchitectureRuleSet> GetRuleSetsByScope(RuleScope scope)
    {
        return RuleSetRegistry.GetByScope(scope);
    }

    public string FormatRuleId(ArchitectureRuleId ruleId)
    {
        // ArchitectureRuleId.ToString() 返回 ADR-XXX_Y_Z 格式
        // 转换为点号格式 ADR-XXX.Y.Z
        return ruleId.ToString().Replace("_", ".");
    }

    public RuleSetSummary CreateSummary(ArchitectureRuleSet ruleSet)
    {
        var severities = ruleSet.Rules
            .Select(r => r.Severity)
            .Distinct()
            .ToList();

        var scopes = ruleSet.Rules
            .Select(r => r.Scope)
            .Distinct()
            .ToList();

        return new RuleSetSummary(
            AdrNumber: ruleSet.AdrNumber,
            FormattedId: $"ADR-{ruleSet.AdrNumber:000}",
            RuleCount: ruleSet.RuleCount,
            ClauseCount: ruleSet.ClauseCount,
            Severities: severities,
            Scopes: scopes);
    }
}
