namespace Zss.BilliardHall.Specification.Services;

/// <summary>
/// 规则集查询服务接口
/// 为 Skills 提供统一的规则集查询和格式化功能
/// 
/// 设计原则：
/// 1. 所有 Skills 通过此服务访问规则集，不直接调用 RuleSetRegistry
/// 2. 提供统一的错误处理和格式化策略
/// 3. 支持严格模式和宽容模式，满足不同场景需求
/// 4. 为失败场景提供明确的诊断信息
/// </summary>
public interface IRuleSetQueryService
{
    /// <summary>
    /// 获取规则集（严格模式）
    /// 适用场景：测试生成、代码生成等必须基于有效规则集的场景
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>规则集</returns>
    /// <exception cref="InvalidOperationException">当规则集不存在时抛出</exception>
    ArchitectureRuleSet GetRuleSetStrict(int adrNumber);

    /// <summary>
    /// 获取规则集（宽容模式）
    /// 适用场景：探索性查询、可选功能等允许失败的场景
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>规则集，如果不存在则返回 null</returns>
    ArchitectureRuleSet? GetRuleSet(int adrNumber);

    /// <summary>
    /// 根据字符串获取规则集（严格模式）
    /// 支持格式：ADR-001, ADR-1, 001, 1
    /// </summary>
    /// <param name="adrId">ADR 编号字符串</param>
    /// <returns>规则集</returns>
    /// <exception cref="ArgumentException">当格式错误时抛出</exception>
    /// <exception cref="InvalidOperationException">当规则集不存在时抛出</exception>
    ArchitectureRuleSet GetRuleSetStrict(string adrId);

    /// <summary>
    /// 获取指定规则（严格模式）
    /// </summary>
    /// <param name="ruleId">规则 ID</param>
    /// <returns>规则定义</returns>
    /// <exception cref="InvalidOperationException">当规则不存在时抛出</exception>
    ArchitectureRuleDefinition GetRuleStrict(ArchitectureRuleId ruleId);

    /// <summary>
    /// 获取指定条款（严格模式）
    /// </summary>
    /// <param name="clauseId">条款 ID</param>
    /// <returns>条款定义</returns>
    /// <exception cref="InvalidOperationException">当条款不存在时抛出</exception>
    ArchitectureClauseDefinition GetClauseStrict(ArchitectureRuleId clauseId);

    /// <summary>
    /// 获取所有规则集
    /// 按 ADR 编号排序
    /// </summary>
    /// <returns>所有已注册的规则集</returns>
    IEnumerable<ArchitectureRuleSet> GetAllRuleSets();

    /// <summary>
    /// 按层级获取规则集
    /// </summary>
    /// <param name="layer">层级名称：Constitutional, Governance, Runtime, Structure</param>
    /// <returns>该层级的所有规则集</returns>
    IEnumerable<ArchitectureRuleSet> GetRuleSetsByLayer(string layer);

    /// <summary>
    /// 按严重程度获取规则集
    /// </summary>
    /// <param name="severity">严重程度</param>
    /// <returns>包含指定严重程度规则的规则集</returns>
    IEnumerable<ArchitectureRuleSet> GetRuleSetsBySeverity(RuleSeverity severity);

    /// <summary>
    /// 按作用域获取规则集
    /// </summary>
    /// <param name="scope">作用域</param>
    /// <returns>包含指定作用域规则的规则集</returns>
    IEnumerable<ArchitectureRuleSet> GetRuleSetsByScope(RuleScope scope);

    /// <summary>
    /// 检查规则集是否存在
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>true 如果规则集已注册</returns>
    bool RuleSetExists(int adrNumber);

    /// <summary>
    /// 格式化规则 ID 为标准格式
    /// 例如：ADR-001_1_2
    /// </summary>
    /// <param name="ruleId">规则 ID</param>
    /// <returns>格式化的规则 ID 字符串</returns>
    string FormatRuleId(ArchitectureRuleId ruleId);

    /// <summary>
    /// 生成规则集摘要信息
    /// 用于日志输出和报告生成
    /// </summary>
    /// <param name="ruleSet">规则集</param>
    /// <returns>包含规则数、条款数等信息的摘要</returns>
    RuleSetSummary GetRuleSetSummary(ArchitectureRuleSet ruleSet);
}

/// <summary>
/// 规则集摘要信息
/// </summary>
public sealed class RuleSetSummary
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber { get; init; }

    /// <summary>
    /// 规则总数
    /// </summary>
    public int RuleCount { get; init; }

    /// <summary>
    /// 条款总数
    /// </summary>
    public int ClauseCount { get; init; }

    /// <summary>
    /// 包含的严重程度列表
    /// </summary>
    public IReadOnlyList<RuleSeverity> Severities { get; init; } = [];

    /// <summary>
    /// 包含的作用域列表
    /// </summary>
    public IReadOnlyList<RuleScope> Scopes { get; init; } = [];

    /// <summary>
    /// 格式化的 ADR 标识符
    /// 例如：ADR-001
    /// </summary>
    public string FormattedAdrId => $"ADR-{AdrNumber:D3}";
}
