using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 架构规则集
/// 把 ADR 变成可执行规范的核心类
/// 
/// 每个 ArchitectureRuleSet 对应一个 ADR 文档，
/// 包含该 ADR 中定义的所有 Rule 和 Clause
/// 
/// 用途：
/// 1. 规则注册和管理
/// 2. 规则查询和访问
/// 3. 未来可用于规则验证和自动化测试生成
/// </summary>
public sealed class ArchitectureRuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber { get; }

    private readonly Dictionary<ArchitectureRuleId, ArchitectureRuleDefinition> _rules = [];
    private readonly Dictionary<ArchitectureRuleId, ArchitectureClauseDefinition> _clauses = [];

    /// <summary>
    /// 创建一个规则集
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    public ArchitectureRuleSet(int adrNumber)
    {
        if (adrNumber <= 0)
        {
            throw new ArgumentException("ADR 编号必须大于 0", nameof(adrNumber));
        }

        AdrNumber = adrNumber;
    }

    /// <summary>
    /// 添加一个规则
    /// </summary>
    /// <param name="ruleNumber">规则编号</param>
    /// <param name="summary">规则摘要</param>
    /// <param name="decision">裁决级别（MUST/MUST_NOT/SHOULD）</param>
    /// <param name="severity">严重程度</param>
    /// <param name="scope">作用域</param>
    public void AddRule(
        int ruleNumber,
        string summary,
        DecisionLevel decision,
        RuleSeverity severity,
        RuleScope scope)
    {
        var id = ArchitectureRuleId.Rule(AdrNumber, ruleNumber);
        var definition = new ArchitectureRuleDefinition(id, summary, decision, severity, scope);
        
        definition.Validate();

        if (_rules.ContainsKey(id))
        {
            throw new InvalidOperationException($"规则 {id} 已存在");
        }

        _rules.Add(id, definition);
    }

    /// <summary>
    /// 添加一个条款
    /// </summary>
    /// <param name="ruleNumber">规则编号</param>
    /// <param name="clauseNumber">条款编号</param>
    /// <param name="condition">条件描述</param>
    /// <param name="enforcement">执行要求</param>
    /// <param name="executionType">执行类型</param>
    public void AddClause(
        int ruleNumber,
        int clauseNumber,
        string condition,
        string enforcement,
        ClauseExecutionType executionType)
    {
        var id = ArchitectureRuleId.Clause(AdrNumber, ruleNumber, clauseNumber);
        var definition = new ArchitectureClauseDefinition(id, condition, enforcement, executionType);
        
        definition.Validate();

        if (_clauses.ContainsKey(id))
        {
            throw new InvalidOperationException($"条款 {id} 已存在");
        }

        _clauses.Add(id, definition);
    }

    /// <summary>
    /// 获取指定规则
    /// </summary>
    public ArchitectureRuleDefinition? GetRule(int ruleNumber)
    {
        var id = ArchitectureRuleId.Rule(AdrNumber, ruleNumber);
        return _rules.GetValueOrDefault(id);
    }

    /// <summary>
    /// 获取指定条款
    /// </summary>
    public ArchitectureClauseDefinition? GetClause(int ruleNumber, int clauseNumber)
    {
        var id = ArchitectureRuleId.Clause(AdrNumber, ruleNumber, clauseNumber);
        return _clauses.GetValueOrDefault(id);
    }

    /// <summary>
    /// 检查规则是否存在
    /// </summary>
    public bool HasRule(int ruleNumber)
    {
        var id = ArchitectureRuleId.Rule(AdrNumber, ruleNumber);
        return _rules.ContainsKey(id);
    }

    /// <summary>
    /// 检查条款是否存在
    /// </summary>
    public bool HasClause(int ruleNumber, int clauseNumber)
    {
        var id = ArchitectureRuleId.Clause(AdrNumber, ruleNumber, clauseNumber);
        return _clauses.ContainsKey(id);
    }

    /// <summary>
    /// 获取所有规则（只读）
    /// </summary>
    public IReadOnlyCollection<ArchitectureRuleDefinition> Rules => _rules.Values;

    /// <summary>
    /// 获取所有条款（只读）
    /// </summary>
    public IReadOnlyCollection<ArchitectureClauseDefinition> Clauses => _clauses.Values;

    /// <summary>
    /// 获取规则数量
    /// </summary>
    public int RuleCount => _rules.Count;

    /// <summary>
    /// 获取条款数量
    /// </summary>
    public int ClauseCount => _clauses.Count;

    /// <summary>
    /// 验证规则集的完整性
    /// 确保每个 Rule 至少有一个对应的 Clause
    /// 
    /// 这是架构治理的关键约束：
    /// "写了 Rule，但永远没人执行"的假治理是不可接受的
    /// 每个规则都必须通过至少一个条款来定义如何执行
    /// </summary>
    /// <exception cref="InvalidOperationException">当存在没有条款的规则时抛出</exception>
    public void ValidateCompleteness()
    {
        var rulesWithoutClauses = new List<ArchitectureRuleId>();

        foreach (var rule in _rules.Values)
        {
            var hasClause = _clauses.Values.Any(c => c.Id.RuleNumber == rule.Id.RuleNumber);
            if (!hasClause)
            {
                rulesWithoutClauses.Add(rule.Id);
            }
        }

        if (rulesWithoutClauses.Any())
        {
            var ruleList = string.Join(", ", rulesWithoutClauses.Select(r => r.ToString()));
            throw new InvalidOperationException(
                $"以下规则没有任何条款，这是不可执行的规则：{ruleList}。" +
                $"每个规则必须至少有一个条款来定义如何执行。");
        }
    }
}
