namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.Tests;

public sealed class Adr907ClauseMetadataInvariants_Tests
{
    [Fact(DisplayName = "不变量：所有条款必须是完整可执行规范")]
    public void ClauseSpec_Should_Always_Be_Complete() =>
        Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Should()
            .AllSatisfy(c =>
            {
                c.Name.Should().NotBeNullOrWhiteSpace();
                c.Description.Should().NotBeNullOrWhiteSpace();
                c.ValidationHint.Should().NotBeNullOrWhiteSpace();
                c.ClauseId.Should().BeGreaterThan(0);
                c.RuleId.Should().BeGreaterThan(0);
            });
}
