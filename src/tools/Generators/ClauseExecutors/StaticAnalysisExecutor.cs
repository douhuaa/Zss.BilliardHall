namespace Zss.BilliardHall.Generators.ClauseExecutors;

using Zss.BilliardHall.Specification.Rules;

/// <summary>
/// 静态分析执行器
/// 生成静态分析相关代码（通常需要 Roslyn Analyzer 支持）
/// </summary>
public sealed class StaticAnalysisExecutor : IClauseExecutor
{
    public ClauseExecutionType SupportedType => ClauseExecutionType.StaticAnalysis;

    public string GenerateAssertionCode(ArchitectureClauseDefinition clause, string indent)
    {
        Guard.NotNull(clause, nameof(clause));
        // 注意：indent 可以是纯空格字符串，这在代码生成中是有效的

        var lines = new List<string>
        {
            $"{indent}// StaticAnalysis 类型：需要 Roslyn Analyzer 支持",
            $"{indent}// 这里可以标记为需要人工验证或跳过",
            $"{indent}Console.WriteLine($\"Rule {{ruleId}} Clause {{clauseId}} 需要 Roslyn Analyzer 支持\");"
        };

        return string.Join("\n", lines);
    }
}
