namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

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
        var ids = input.Select(ArchitectureRuleId.Parse);

        ids.OrderBy(x => x)
            .Select(x => x.ToString())
            .Should()
            .Equal(expected);
    }

    [Theory(DisplayName = "不变量：同编号下 Rule 永远排在 Clause 之前")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    public void Rule_Should_Always_Come_Before_Its_Clause(int adr, int rule, int clause)
    {
        var ruleId = ArchitectureRuleId.Rule(adr, rule);
        var clauseId = ArchitectureRuleId.Clause(adr, rule, clause);

        ruleId.CompareTo(clauseId).Should().BeLessThan(0);
    }
}