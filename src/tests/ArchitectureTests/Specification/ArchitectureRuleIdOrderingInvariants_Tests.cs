namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// ArchitectureRuleId 排序不变量测试
/// 验证 RuleId 的排序逻辑是否符合预期
/// 
/// 核心不变量：
/// - 排序顺序：ADR 编号 → Rule 编号 → Clause 编号
/// - 同一 Rule 下，Rule 级别永远排在 Clause 级别之前
/// - 排序结果应该稳定且可预测
/// </summary>
public sealed class ArchitectureRuleIdOrderingInvariants_Tests
{
    [Theory(DisplayName = "不变量：排序顺序为 ADR → Rule → Clause")]
    [InlineData(
        new[] { "ADR-907_3_2", "ADR-907_1", "ADR-907_3_1", "ADR-900_1", "ADR-900_1_1" },
        new[] { "ADR-900_1", "ADR-900_1_1", "ADR-907_1", "ADR-907_3_1", "ADR-907_3_2" }
    )]
    public void RuleIds_Should_Sort_By_Adr_Then_Rule_Then_Clause(
        string[] input,
        string[] expected)
    {
        var ids = input.Select(ArchitectureRuleId.Parse).ToList();

        var sorted = ids.OrderBy(x => x)
            .Select(x => x.ToString())
            .ToList();

        sorted.Should().Equal(expected, "排序应按 ADR → Rule → Clause 的顺序");
    }

    [Theory(DisplayName = "不变量：同编号下 Rule 永远排在 Clause 之前")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    public void Rule_Should_Always_Come_Before_Its_Clause(int adr, int rule, int clause)
    {
        var ruleId = ArchitectureRuleId.Rule(adr, rule);
        var clauseId = ArchitectureRuleId.Clause(adr, rule, clause);

        ruleId.CompareTo(clauseId).Should().BeLessThan(0, 
            $"{ruleId} 应该排在 {clauseId} 之前");
    }
}