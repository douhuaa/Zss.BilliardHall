namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.Tests;

public sealed class Adr907ClauseTopologyInvariants_Tests
{
    [Fact(DisplayName = "不变量：条款 (RuleId, ClauseId) 在 ADR-907 内唯一")]
    public void Clause_Identity_Should_Be_Globally_Unique() =>
        Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Select(c => (c.RuleId, c.ClauseId))
            .Should()
            .OnlyHaveUniqueItems();

    [Fact(DisplayName = "不变量：条款的 RuleId 必须与父规则一致")]
    public void Clause_RuleId_Must_Match_Parent_Rule() =>
        Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses, (r, c) => (Rule: r, Clause: c))
            .Should()
            .AllSatisfy(x =>
                x.Clause.RuleId.Should().Be(x.Rule.RuleId));
}
