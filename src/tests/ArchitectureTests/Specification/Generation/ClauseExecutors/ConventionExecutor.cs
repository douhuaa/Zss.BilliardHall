namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.ClauseExecutors;

/// <summary>
/// 约定检查执行器
/// 生成 NetArchTest 约定检查代码
/// </summary>
public sealed class ConventionExecutor : IClauseExecutor
{
    public ClauseExecutionType SupportedType => ClauseExecutionType.Convention;

    public string GenerateAssertionCode(ArchitectureClauseDefinition clause, string indent)
    {
        Guard.NotNull(clause, nameof(clause));
        // 注意：indent 可以是纯空格字符串，这在代码生成中是有效的

        var lines = new List<string>
        {
            $"{indent}// Convention 类型：使用 NetArchTest 进行架构约定检查",
            $"{indent}ExecuteConventionTest(ruleIdStr, clause);"
        };

        return string.Join("\n", lines);
    }
}
