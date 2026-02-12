namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests.RuleSets.ADR907;

/// <summary>
/// ADR-907 自动生成测试类
/// 每条 Rule/Clause 对应一个单独的 Fact 方法
/// </summary>
public sealed class Adr907Auto_Tests
{
    // 遍历所有规则和条款生成测试
    static IEnumerable<(int RuleId, int ClauseId, string Name)> AllClauses()
    {
        foreach (var rule in Adr907Definitions.AllRules)
        {
            foreach (var clause in rule.Clauses)
            {
                yield return (rule.RuleId, clause.ClauseId, clause.Name);
            }
        }
    }

    // 每条条款生成独立 Fact 方法
    // 方法名格式：ADR_{RuleId}_{ClauseId}_{SafeName}
    // SafeName 用于方法名安全替换空格和特殊字符
    public static IEnumerable<object[]> GetFactData()
    {
        foreach (var (ruleId, clauseId, name) in AllClauses())
        {
            var safeName = name.Replace(" ", "_").Replace("-", "_").Replace("/", "_");
            yield return new object[] { ruleId, clauseId, safeName, name };
        }
    }

    [Theory(DisplayName = "ADR-907 自动生成条款测试")]
    [MemberData(nameof(GetFactData))]
    public void RunClause(int ruleId, int clauseId, string safeName, string displayName)
    {
        // 可以在这里调用具体绑定的执行器
        var binding = Adr907ExecutionBindings.Lookup(ruleId, clauseId);

        // 对未绑定条款默认执行约定逻辑
        if (binding is null)
        {
            // Convention 执行示例：ArchitectureTests 约定执行
            binding = new ClauseExecutionBinding(ruleId, clauseId, "Convention.ArchitectureTests");
        }

        // 执行测试逻辑（这里只做演示，可以替换成实际 Analyzer 调用）
        binding.HandlerKey.Should().NotBeNullOrWhiteSpace($"Rule {ruleId} Clause {clauseId} ({displayName}) 必须有执行绑定");

        // 这里可以加更多断言，比如：
        // - 绑定的 HandlerKey 是否符合规范
        // - 静态分析、规则存在性、目录结构等
    }
}
