namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.Tests;

public sealed class Adr907RuleIdentityInvariants_Tests
{
    [Fact(DisplayName = "不变量：每个规则的 RuleId 在 ADR-907 内唯一")]
    public void RuleIds_Should_Be_Unique_Within_Adr() =>
        Adr907Definitions.AllRules
            .Select(r => r.RuleId)
            .Should()
            .OnlyHaveUniqueItems();

    [Fact(DisplayName = "不变量：每个规则至少定义一个条款")]
    public void Every_Rule_Should_Have_At_Least_One_Clause() =>
        Adr907Definitions.AllRules
            .Should()
            .AllSatisfy(r =>
                r.Clauses.Should().NotBeEmpty(
                $"规则 {r.RuleId} 必须通过条款定义执行语义"));
}
