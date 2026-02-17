using Zss.BilliardHall.Specification.Index;

namespace Zss.BilliardHall.Specification.Services;

/// <summary>
/// 规则集查询服务实现
/// 基于 RuleSetRegistry 提供统一的规则集查询和格式化功能
/// </summary>
public sealed class RuleSetQueryService : IRuleSetQueryService
{
    /// <inheritdoc />
    public ArchitectureRuleSet GetRuleSetStrict(int adrNumber)
    {
        return RuleSetRegistry.GetStrict(adrNumber);
    }

    /// <inheritdoc />
    public ArchitectureRuleSet? GetRuleSet(int adrNumber)
    {
        return RuleSetRegistry.Get(adrNumber);
    }

    /// <inheritdoc />
    public ArchitectureRuleSet GetRuleSetStrict(string adrId)
    {
        return RuleSetRegistry.GetStrict(adrId);
    }

    /// <inheritdoc />
    public ArchitectureRuleDefinition GetRuleStrict(ArchitectureRuleId ruleId)
    {
        var ruleSet = GetRuleSetStrict(ruleId.AdrNumber);
        var rule = ruleSet.GetRule(ruleId.RuleNumber);

        if (rule == null)
        {
            throw new InvalidOperationException(
                $"规则 {FormatRuleId(ruleId)} 不存在于 ADR-{ruleId.AdrNumber:D3} 中。" +
                $"可用的规则：{string.Join(", ", ruleSet.Rules.Select(r => FormatRuleId(r.Id)))}");
        }

        return rule;
    }

    /// <inheritdoc />
    public ArchitectureClauseDefinition GetClauseStrict(ArchitectureRuleId clauseId)
    {
        if (!clauseId.IsClause)
        {
            throw new ArgumentException(
                $"提供的 RuleId {FormatRuleId(clauseId)} 不是条款 ID。" +
                $"请使用 ArchitectureRuleId.Clause() 创建条款 ID。",
                nameof(clauseId));
        }

        var ruleSet = GetRuleSetStrict(clauseId.AdrNumber);
        var clause = ruleSet.GetClause(clauseId.RuleNumber, clauseId.ClauseNumber ?? 0);

        if (clause == null)
        {
            throw new InvalidOperationException(
                $"条款 {FormatRuleId(clauseId)} 不存在于 ADR-{clauseId.AdrNumber:D3} 中。" +
                $"可用的条款：{string.Join(", ", ruleSet.Clauses.Select(c => FormatRuleId(c.Id)))}");
        }

        return clause;
    }

    /// <inheritdoc />
    public IEnumerable<ArchitectureRuleSet> GetAllRuleSets()
    {
        return RuleSetRegistry.GetAllRuleSets();
    }

    /// <inheritdoc />
    public IEnumerable<ArchitectureRuleSet> GetRuleSetsByLayer(string layer)
    {
        return layer.ToLowerInvariant() switch
        {
            "constitutional" => RuleSetRegistry.GetConstitutionalRuleSets(),
            "governance" => RuleSetRegistry.GetGovernanceRuleSets(),
            "runtime" => RuleSetRegistry.GetRuntimeRuleSets(),
            "structure" => RuleSetRegistry.GetStructureRuleSets(),
            _ => throw new ArgumentException(
                $"未知的层级：{layer}。支持的层级：Constitutional, Governance, Runtime, Structure",
                nameof(layer))
        };
    }

    /// <inheritdoc />
    public IEnumerable<ArchitectureRuleSet> GetRuleSetsBySeverity(RuleSeverity severity)
    {
        return RuleSetRegistry.GetBySeverity(severity);
    }

    /// <inheritdoc />
    public IEnumerable<ArchitectureRuleSet> GetRuleSetsByScope(RuleScope scope)
    {
        return RuleSetRegistry.GetByScope(scope);
    }

    /// <inheritdoc />
    public bool RuleSetExists(int adrNumber)
    {
        return RuleSetRegistry.Contains(adrNumber);
    }

    /// <inheritdoc />
    public string FormatRuleId(ArchitectureRuleId ruleId)
    {
        return ruleId.ToString();
    }

    /// <inheritdoc />
    public RuleSetSummary GetRuleSetSummary(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);

        return new RuleSetSummary
        {
            AdrNumber = ruleSet.AdrNumber,
            RuleCount = ruleSet.RuleCount,
            ClauseCount = ruleSet.ClauseCount,
            Severities = ruleSet.Rules
                .Select(r => r.Severity)
                .Distinct()
                .OrderBy(s => s)
                .ToList(),
            Scopes = ruleSet.Rules
                .Select(r => r.Scope)
                .Distinct()
                .OrderBy(s => s)
                .ToList()
        };
    }
}
