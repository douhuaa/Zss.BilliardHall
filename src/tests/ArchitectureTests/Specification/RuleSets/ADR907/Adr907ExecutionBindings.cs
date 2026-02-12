namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907 执行绑定
/// 定义条款到具体执行处理器的绑定关系
/// 
/// 这是规范与执行分离的关键：
/// - ClauseSpec 定义"是什么"（声明式规范）
/// - ClauseExecutionBinding 定义"如何执行"（执行绑定）
/// - 本类提供绑定的查找和管理
/// 
/// 设计说明：
/// - 初始阶段仅包含少量示例绑定
/// - 未来可扩展为从配置文件或数据库加载
/// - 未绑定的条款会使用约定（Convention）执行
/// </summary>
public static class Adr907ExecutionBindings
{
    /// <summary>
    /// 所有执行绑定
    /// 格式：(RuleId, ClauseId) -> HandlerKey
    /// </summary>
    private static readonly ClauseExecutionBinding[] Bindings =
    [
        // 示例绑定：Rule 1, Clause 1 -> Analyzer 强制架构测试存在
        new ClauseExecutionBinding(
            RuleId: 1,
            ClauseId: 1,
            HandlerKey: "Analyzer.Enforce.ArchitectureTestPresence"),

        // 示例绑定：Rule 2, Clause 7 -> Analyzer 检测弱断言
        new ClauseExecutionBinding(
            RuleId: 2,
            ClauseId: 7,
            HandlerKey: "Analyzer.Detect.WeakAssertions")

        // 未来可添加更多绑定
        // 如: new ClauseExecutionBinding(3, 1, "Analyzer.StaticAnalysis.AssertionCount")
    ];

    /// <summary>
    /// 查找指定条款的执行绑定
    /// </summary>
    /// <param name="ruleId">规则编号</param>
    /// <param name="clauseId">条款编号</param>
    /// <returns>执行绑定，如果不存在则返回 null</returns>
    public static ClauseExecutionBinding? Lookup(int ruleId, int clauseId)
    {
        return Bindings.FirstOrDefault(b => b.RuleId == ruleId && b.ClauseId == clauseId);
    }

    /// <summary>
    /// 获取所有绑定（只读）
    /// </summary>
    public static IReadOnlyCollection<ClauseExecutionBinding> All => Bindings;

    /// <summary>
    /// 获取绑定数量
    /// </summary>
    public static int Count => Bindings.Length;
}
