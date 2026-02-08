namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

public sealed class ArchitectureRuleIdRepresentationInvariants_Tests
{
    [Theory(DisplayName = "不变量：RuleId 的字符串表示是稳定规范格式")]
    [InlineData(907, 3, "ADR-907_3")]
    [InlineData(1, 1, "ADR-001_1")]
    [InlineData(900, 2, "ADR-900_2")]
    public void RuleId_ToString_Should_Follow_Spec(int adr, int rule, string expected)
    {
        var id = ArchitectureRuleId.Rule(adr, rule);

        id.ToString().Should().Be(expected);
    }

    [Theory(DisplayName = "不变量：ClauseId 的字符串表示是稳定规范格式")]
    [InlineData(907, 3, 1, "ADR-907_3_1")]
    [InlineData(900, 1, 1, "ADR-900_1_1")]
    public void ClauseId_ToString_Should_Follow_Spec(int adr, int rule, int clause, string expected)
    {
        var id = ArchitectureRuleId.Clause(adr, rule, clause);

        id.ToString().Should().Be(expected);
    }
}