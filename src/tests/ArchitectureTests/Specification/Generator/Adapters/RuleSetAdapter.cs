using Zss.BilliardHall.Generators;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Adapters;

/// <summary>
/// ArchitectureRuleSet 到 IRuleSet 的适配器
/// </summary>
internal sealed class RuleSetAdapter : IRuleSet
{
    private readonly ArchitectureRuleSet _ruleSet;
    private IReadOnlyList<IRule>? _rules;
    private IReadOnlyList<IClause>? _clauses;

    public RuleSetAdapter(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        _ruleSet = ruleSet;
    }

    public int AdrNumber => _ruleSet.AdrNumber;

    public IReadOnlyList<IRule> Rules =>
        _rules ??= _ruleSet.Rules.Select(r => new RuleAdapter(r)).ToList();

    public IReadOnlyList<IClause> Clauses =>
        _clauses ??= _ruleSet.Clauses.Select(c => new ClauseAdapter(c)).ToList();
}

/// <summary>
/// ArchitectureRuleDefinition 到 IRule 的适配器
/// </summary>
internal sealed class RuleAdapter : IRule
{
    private readonly ArchitectureRuleDefinition _rule;
    private IRuleId? _id;

    public RuleAdapter(ArchitectureRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rule = rule;
    }

    public IRuleId Id => _id ??= new RuleIdAdapter(_rule.Id);

    public string Summary => _rule.Summary;
}

/// <summary>
/// ArchitectureClauseDefinition 到 IClause 的适配器
/// </summary>
internal sealed class ClauseAdapter : IClause
{
    private readonly ArchitectureClauseDefinition _clause;
    private IRuleId? _id;

    public ClauseAdapter(ArchitectureClauseDefinition clause)
    {
        ArgumentNullException.ThrowIfNull(clause);
        _clause = clause;
    }

    public IRuleId Id => _id ??= new RuleIdAdapter(_clause.Id);

    public string Condition => _clause.Condition;

    public string Enforcement => _clause.Enforcement;
}

/// <summary>
/// ArchitectureRuleId 到 IRuleId 的适配器
/// </summary>
internal sealed class RuleIdAdapter : IRuleId
{
    private readonly ArchitectureRuleId _id;

    public RuleIdAdapter(ArchitectureRuleId id)
    {
        _id = id;
    }

    public int AdrNumber => _id.AdrNumber;

    public int RuleNumber => _id.RuleNumber;

    public int? ClauseNumber => _id.ClauseNumber;

    public override string ToString() => _id.ToString();
}
