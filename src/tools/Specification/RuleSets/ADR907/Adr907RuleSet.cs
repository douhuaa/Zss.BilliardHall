namespace Zss.BilliardHall.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907：ArchitectureTests 执法治理体系
/// 定义架构测试的命名、组织、断言等规则
/// 
/// 重构说明：
/// - 使用策略模式替代 switch 语句
/// - 从 Adr907Definitions 加载规则定义
/// - 通过 ClauseRegistrationStrategyResolver 解析注册策略
/// - 支持执行绑定查找（Adr907ExecutionBindings）
/// </summary>
public sealed class Adr907RuleSet : IArchitectureRuleSetDefinition
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public int AdrNumber => Adr907Definitions.AdrId;

    /// <summary>
    /// 定义完整的规则集
    /// </summary>
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(Adr907Definitions.AdrId);

        // 从 Adr907Definitions 加载所有规则
        foreach (var ruleInfo in Adr907Definitions.AllRules)
        {
            // 添加规则
            ruleSet.AddRule(
                ruleNumber: ruleInfo.RuleId,
                summary: ruleInfo.Summary,
                decision: ruleInfo.Decision,
                severity: ruleInfo.Severity,
                scope: ruleInfo.Scope);

            // 注册该规则下的所有条款
            foreach (var clauseSpec in ruleInfo.Clauses)
            {
                RegisterClause(ruleSet, clauseSpec);
            }
        }

        return ruleSet;
    });

    /// <summary>
    /// 注册单个条款
    /// 使用策略模式替代 switch 语句
    /// </summary>
    private static void RegisterClause(ArchitectureRuleSet ruleSet, ClauseSpec spec)
    {
        // 验证条款规范
        spec.Validate();

        // 验证条款的 RuleId 与父规则一致
        // 这是架构约束：条款必须属于其声明的规则
        if (!ruleSet.HasRule(spec.RuleId))
        {
            throw new ArgumentException(
                $"条款 ({spec.RuleId},{spec.ClauseId}) 的 RuleId {spec.RuleId} " +
                $"在 ADR-{ruleSet.AdrNumber} 中不存在。" +
                $"请先添加规则 {spec.RuleId}。",
                nameof(spec));
        }

        // 查找执行绑定
        var binding = Adr907ExecutionBindings.Lookup(spec.RuleId, spec.ClauseId);

        // 解析注册策略
        var strategy = ClauseRegistrationStrategyResolver.Resolve(spec.ExecutionType);

        // 执行注册
        strategy.Register(ruleSet, spec, binding);
    }
}
