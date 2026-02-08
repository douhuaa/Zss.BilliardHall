namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

public sealed class ArchitectureRuleIdIdentityInvariants_Tests
{
    [Theory(DisplayName = "不变量：RuleId 表示 ADR 下的规则级别")]
    [InlineData(907, 3)]
    [InlineData(1, 1)]
    [InlineData(900, 2)]
    public void RuleId_Should_Always_Be_Rule_Level(int adr, int rule)
    {
        var id = ArchitectureRuleId.Rule(adr, rule);

        id.Level.Should().Be(RuleLevel.Rule);
        id.ClauseNumber.Should().BeNull();
    }

    [Theory(DisplayName = "不变量：ClauseId 表示 ADR 下的子规则级别")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 2, 5)]
    public void ClauseId_Should_Always_Be_Clause_Level(int adr, int rule, int clause)
    {
        var id = ArchitectureRuleId.Clause(adr, rule, clause);

        id.Level.Should().Be(RuleLevel.Clause);
        id.ClauseNumber.Should().Be(clause);
    }
}
