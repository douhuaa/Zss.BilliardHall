namespace Zss.BilliardHall.Generators.ClauseExecutors;
using Zss.BilliardHall.Specification.Rules;

/// <summary>
/// 手工审查执行器
/// 生成手工审查标记代码
/// </summary>
public sealed class ManualReviewExecutor : IClauseExecutor
{
    public ClauseExecutionType SupportedType => ClauseExecutionType.ManualReview;

    public string GenerateAssertionCode(ArchitectureClauseDefinition clause, string indent)
    {
        Guard.NotNull(clause, nameof(clause));
        // 注意：indent 可以是纯空格字符串，这在代码生成中是有效的

        var lines = new List<string>
        {
            $"{indent}// ManualReview 类型：需要人工审查",
            $"{indent}Console.WriteLine($\"Rule {{ruleId}} Clause {{clauseId}} 需要人工审查\");"
        };

        return string.Join("\n", lines);
    }
}
