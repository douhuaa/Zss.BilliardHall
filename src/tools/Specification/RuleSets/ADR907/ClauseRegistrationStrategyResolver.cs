namespace Zss.BilliardHall.Specification.RuleSets.ADR907;

/// <summary>
/// 条款注册策略解析器
/// 根据 ClauseExecutionType 解析到对应的注册策略
/// 
/// 这是策略模式的核心：替代 switch 语句，提供可扩展的策略映射
/// </summary>
public static class ClauseRegistrationStrategyResolver
{
    // 策略实例缓存（单例模式）
    private static readonly ConventionRegistrationStrategy ConventionStrategy = new();
    private static readonly StaticAnalysisRegistrationStrategy StaticAnalysisStrategy = new();
    private static readonly DocumentationRegistrationStrategy DocumentationStrategy = new();
    private static readonly ManualReviewRegistrationStrategy ManualReviewStrategy = new();

    // 策略映射表
    private static readonly Dictionary<ClauseExecutionType, IClauseRegistrationStrategy> StrategyMap = new()
    {
        { ClauseExecutionType.Convention, ConventionStrategy },
        { ClauseExecutionType.StaticAnalysis, StaticAnalysisStrategy },
        { ClauseExecutionType.Documentation, DocumentationStrategy },
        { ClauseExecutionType.ManualReview, ManualReviewStrategy },
        // Runtime 类型降级为 Convention（保持向后兼容）
        { ClauseExecutionType.Runtime, ConventionStrategy }
    };

    /// <summary>
    /// 解析 ClauseExecutionType 到对应的注册策略
    /// </summary>
    /// <param name="executionType">执行类型</param>
    /// <returns>对应的注册策略</returns>
    /// <exception cref="ArgumentException">当执行类型不支持时抛出</exception>
    public static IClauseRegistrationStrategy Resolve(ClauseExecutionType executionType)
    {
        if (StrategyMap.TryGetValue(executionType, out var strategy))
        {
            return strategy;
        }

        throw new ArgumentException(
            $"不支持的执行类型: {executionType}。" +
            $"支持的类型: {string.Join(", ", StrategyMap.Keys)}",
            nameof(executionType));
    }

    /// <summary>
    /// 检查是否支持指定的执行类型
    /// </summary>
    public static bool IsSupported(ClauseExecutionType executionType)
    {
        return StrategyMap.ContainsKey(executionType);
    }

    /// <summary>
    /// 获取所有支持的执行类型
    /// </summary>
    public static IReadOnlyCollection<ClauseExecutionType> SupportedTypes => StrategyMap.Keys;
}
