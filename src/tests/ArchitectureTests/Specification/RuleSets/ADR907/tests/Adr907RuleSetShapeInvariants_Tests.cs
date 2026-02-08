namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.Tests;

public sealed class Adr907RuleSetShapeInvariants_Tests
{
    [Fact(DisplayName = "不变量：ADR-907 定义的是一个封闭的 4 规则体系")]
    public void Adr907_Should_Define_Four_And_Only_Four_Rules() =>
        Adr907Definitions.AllRules
            .Select(r => r.RuleId)
            .Should()
            .BeEquivalentTo([1, 2, 3, 4],
            "ADR-907 的规则编号是稳定且封闭的");
}
