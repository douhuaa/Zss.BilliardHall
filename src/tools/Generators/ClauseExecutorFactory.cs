using Zss.BilliardHall.Generators.ClauseExecutors;
using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Specification.Language.RuleIdLanguage;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// 条款执行器工厂实现
/// 使用策略模式管理不同执行类型的执行器
/// </summary>
public sealed class ClauseExecutorFactory : IClauseExecutorFactory
{
    private readonly Dictionary<ClauseExecutionType, IClauseExecutor> _executors;

    public ClauseExecutorFactory()
    {
        _executors = new Dictionary<ClauseExecutionType, IClauseExecutor>
        {
            [ClauseExecutionType.Convention] = new ConventionExecutor(),
            [ClauseExecutionType.StaticAnalysis] = new StaticAnalysisExecutor(),
            [ClauseExecutionType.Runtime] = new RuntimeExecutor(),
            [ClauseExecutionType.Documentation] = new DocumentationExecutor(),
            [ClauseExecutionType.ManualReview] = new ManualReviewExecutor()
        };
    }

    public IClauseExecutor GetExecutor(ClauseExecutionType executionType)
    {
        if (_executors.TryGetValue(executionType, out var executor))
        {
            return executor;
        }

        throw new NotSupportedException($"不支持的执行类型: {executionType}");
    }
}
