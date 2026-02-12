namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.ClauseExecutors;

/// <summary>
/// 运行时验证执行器
/// 生成运行时验证代码
/// </summary>
public sealed class RuntimeExecutor : IClauseExecutor
{
    public ClauseExecutionType SupportedType => ClauseExecutionType.Runtime;

    public string GenerateAssertionCode(ArchitectureClauseDefinition clause, string indent)
    {
        Guard.NotNull(clause, nameof(clause));
        // 注意：indent 可以是纯空格字符串，这在代码生成中是有效的

        var lines = new List<string>
        {
            $"{indent}// Runtime 类型：运行时验证",
            $"{indent}Console.WriteLine($\"Rule {{ruleId}} Clause {{clauseId}} 需要运行时验证\");"
        };

        return string.Join("\n", lines);
    }
}
