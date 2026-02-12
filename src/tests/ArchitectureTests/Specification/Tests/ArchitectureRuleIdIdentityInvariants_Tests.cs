namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// ArchitectureRuleId 身份不变量测试
/// 验证 RuleId 和 ClauseId 的级别标识是否正确
/// 
/// 核心不变量：
/// - RuleId 永远是 Rule 级别（ClauseNumber == null）
/// - ClauseId 永远是 Clause 级别（ClauseNumber != null）
/// </summary>
public sealed class ArchitectureRuleIdIdentityInvariants_Tests
{
    [Theory(DisplayName = "不变量：RuleId 表示 ADR 下的规则级别")]
    [InlineData(907, 3)]
    [InlineData(1, 1)]
    [InlineData(900, 2)]
    public void RuleId_Should_Always_Be_Rule_Level(int adr, int rule)
    {
        var id = ArchitectureRuleId.Rule(adr, rule);

        RuleIdAssertions.AssertIsRule(id, context: $"RuleId({adr}, {rule})");
    }

    [Theory(DisplayName = "不变量：ClauseId 表示 ADR 下的子规则级别")]
    [InlineData(907, 3, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(900, 2, 5)]
    public void ClauseId_Should_Always_Be_Clause_Level(int adr, int rule, int clause)
    {
        var id = ArchitectureRuleId.Clause(adr, rule, clause);

        RuleIdAssertions.AssertIsClause(id, context: $"ClauseId({adr}, {rule}, {clause})");
    }
}
