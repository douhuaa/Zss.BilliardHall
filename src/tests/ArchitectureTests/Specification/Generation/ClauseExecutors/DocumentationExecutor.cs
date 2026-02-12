namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.ClauseExecutors;

/// <summary>
/// 文档验证执行器
/// 生成文档验证代码
/// </summary>
public sealed class DocumentationExecutor : IClauseExecutor
{
    public ClauseExecutionType SupportedType => ClauseExecutionType.Documentation;

    public string GenerateAssertionCode(ArchitectureClauseDefinition clause, string indent)
    {
        Guard.NotNull(clause, nameof(clause));
        // 注意：indent 可以是纯空格字符串，这在代码生成中是有效的

        var lines = new List<string>
        {
            $"{indent}// Documentation 类型：文档验证",
            $"{indent}Console.WriteLine($\"Rule {{ruleId}} Clause {{clauseId}} 需要文档验证\");"
        };

        return string.Join("\n", lines);
    }
}
