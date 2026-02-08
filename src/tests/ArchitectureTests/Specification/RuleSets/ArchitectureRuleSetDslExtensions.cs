namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets;

/// <summary>
/// DSL 扩展方法，用于以声明式方式定义架构规则集
/// 提供链式 API：Rule(...).Clause(...) 以减少样板代码
/// </summary>
public static class ArchitectureRuleSetDslExtensions
{
    /// <summary>
    /// 开始定义新规则
    /// </summary>
    /// <param name="ruleSet">规则集实例</param>
    /// <param name="ruleId">规则唯一标识符（如 "ADR-907_1"）</param>
    /// <param name="title">规则标题</param>
    /// <param name="description">规则描述</param>
    /// <returns>规则构建器，支持链式调用添加条款</returns>
    public static RuleBuilder Rule(
        this ArchitectureRuleSet ruleSet,
        string ruleId,
        string title,
        string? description = null)
    {
        return new RuleBuilder(ruleSet, ruleId, title, description);
    }
}

/// <summary>
/// 规则构建器，支持链式添加各类条款
/// </summary>
public class RuleBuilder
{
    private readonly ArchitectureRuleSet _ruleSet;
    private readonly string _ruleId;
    private readonly string _title;
    private readonly string? _description;

    internal RuleBuilder(ArchitectureRuleSet ruleSet, string ruleId, string title, string? description)
    {
        _ruleSet = ruleSet;
        _ruleId = ruleId;
        _title = title;
        _description = description;
        
        // 立即添加规则到规则集
        _ruleSet.AddRule(_ruleId, _title, _description);
    }

    /// <summary>
    /// 添加通用条款
    /// </summary>
    /// <param name="clauseId">条款编号（如 "1"）</param>
    /// <param name="title">条款标题</param>
    /// <param name="condition">条款条件描述</param>
    /// <param name="enforcement">执法级别（如 "L1", "L2"）</param>
    /// <param name="executionType">执行类型（如 "Convention", "Static", "Documentation", "ManualReview"）</param>
    /// <returns>规则构建器，支持继续链式调用</returns>
    public RuleBuilder Clause(
        string clauseId,
        string title,
        string condition,
        string enforcement = "L1",
        string executionType = "Convention")
    {
        _ruleSet.AddClause(_ruleId, clauseId, title, condition, enforcement, executionType);
        return this;
    }

    /// <summary>
    /// 添加约定条款（Convention Clause）
    /// 用于基于约定的架构约束验证
    /// </summary>
    /// <param name="clauseId">条款编号</param>
    /// <param name="title">条款标题</param>
    /// <param name="condition">条款条件描述</param>
    /// <param name="enforcement">执法级别（默认 L1）</param>
    /// <returns>规则构建器</returns>
    public RuleBuilder ConventionClause(
        string clauseId,
        string title,
        string condition,
        string enforcement = "L1")
    {
        return Clause(clauseId, title, condition, enforcement, "Convention");
    }

    /// <summary>
    /// 添加静态分析条款（Static Clause）
    /// 用于基于静态代码分析的约束验证
    /// </summary>
    /// <param name="clauseId">条款编号</param>
    /// <param name="title">条款标题</param>
    /// <param name="condition">条款条件描述</param>
    /// <param name="enforcement">执法级别（默认 L1）</param>
    /// <returns>规则构建器</returns>
    public RuleBuilder StaticClause(
        string clauseId,
        string title,
        string condition,
        string enforcement = "L1")
    {
        return Clause(clauseId, title, condition, enforcement, "Static");
    }

    /// <summary>
    /// 添加文档验证条款（Documentation Clause）
    /// 用于基于文档结构和内容的约束验证
    /// </summary>
    /// <param name="clauseId">条款编号</param>
    /// <param name="title">条款标题</param>
    /// <param name="condition">条款条件描述</param>
    /// <param name="enforcement">执法级别（默认 L1）</param>
    /// <returns>规则构建器</returns>
    public RuleBuilder DocumentationClause(
        string clauseId,
        string title,
        string condition,
        string enforcement = "L1")
    {
        return Clause(clauseId, title, condition, enforcement, "Documentation");
    }

    /// <summary>
    /// 添加人工审查条款（Manual Review Clause）
    /// 用于需要人工判断的约束验证
    /// </summary>
    /// <param name="clauseId">条款编号</param>
    /// <param name="title">条款标题</param>
    /// <param name="condition">条款条件描述</param>
    /// <param name="enforcement">执法级别（默认 L2）</param>
    /// <returns>规则构建器</returns>
    public RuleBuilder ManualReviewClause(
        string clauseId,
        string title,
        string condition,
        string enforcement = "L2")
    {
        return Clause(clauseId, title, condition, enforcement, "ManualReview");
    }
}

/// <summary>
/// 架构规则集基类
/// 提供规则和条款的管理功能
/// </summary>
public abstract class ArchitectureRuleSet
{
    private readonly Dictionary<string, ArchitectureRule> _rules = new();

    /// <summary>
    /// ADR 编号（如 "ADR-907"）
    /// </summary>
    public string AdrNumber { get; protected set; } = string.Empty;

    /// <summary>
    /// 规则集标题
    /// </summary>
    public string Title { get; protected set; } = string.Empty;

    /// <summary>
    /// 规则集描述
    /// </summary>
    public string? Description { get; protected set; }

    /// <summary>
    /// 所有已定义的规则
    /// </summary>
    public IReadOnlyDictionary<string, ArchitectureRule> Rules => _rules;

    /// <summary>
    /// 添加规则（仅供内部使用，外部应使用 DSL 扩展方法）
    /// </summary>
    protected internal void AddRule(string ruleId, string title, string? description = null)
    {
        if (!_rules.ContainsKey(ruleId))
        {
            _rules[ruleId] = new ArchitectureRule
            {
                RuleId = ruleId,
                Title = title,
                Description = description
            };
        }
    }

    /// <summary>
    /// 添加条款（仅供内部使用，外部应使用 DSL 扩展方法）
    /// </summary>
    protected internal void AddClause(
        string ruleId,
        string clauseId,
        string title,
        string condition,
        string enforcement,
        string executionType)
    {
        // 确保规则存在
        if (!_rules.ContainsKey(ruleId))
        {
            AddRule(ruleId, $"Rule {ruleId}");
        }

        var rule = _rules[ruleId];
        var fullClauseId = $"{ruleId}_{clauseId}";

        rule.Clauses[fullClauseId] = new ArchitectureClause
        {
            ClauseId = fullClauseId,
            Title = title,
            Condition = condition,
            Enforcement = enforcement,
            ExecutionType = executionType
        };
    }

    /// <summary>
    /// 初始化规则集（由子类实现）
    /// </summary>
    protected abstract void DefineRules();

    /// <summary>
    /// 延迟初始化，确保规则只定义一次
    /// </summary>
    protected void EnsureInitialized()
    {
        if (_rules.Count == 0)
        {
            DefineRules();
        }
    }
}

/// <summary>
/// 架构规则定义
/// </summary>
public class ArchitectureRule
{
    /// <summary>
    /// 规则唯一标识符（如 "ADR-907_1"）
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// 规则标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 规则描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 规则下的所有条款
    /// </summary>
    public Dictionary<string, ArchitectureClause> Clauses { get; } = new();
}

/// <summary>
/// 架构条款定义
/// </summary>
public class ArchitectureClause
{
    /// <summary>
    /// 条款唯一标识符（如 "ADR-907_1_1"）
    /// </summary>
    public string ClauseId { get; set; } = string.Empty;

    /// <summary>
    /// 条款标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 条款条件描述
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// 执法级别（L1, L2 等）
    /// </summary>
    public string Enforcement { get; set; } = "L1";

    /// <summary>
    /// 执行类型（Convention, Static, Documentation, ManualReview）
    /// </summary>
    public string ExecutionType { get; set; } = "Convention";
}
