namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// ArchitectureRuleId 字符串表示不变量测试
/// 验证 RuleId 的 ToString() 方法输出的规范性和稳定性
/// 
/// 核心不变量：
/// - ToString() 应该返回规范的格式
/// - Rule 格式：ADR-{编号}_{Rule}（如 ADR-907_3）
/// - Clause 格式：ADR-{编号}_{Rule}_{Clause}（如 ADR-907_3_1）
/// - ADR 编号始终填充为 3 位数（如 001, 907）
/// </summary>
public sealed class ArchitectureRuleIdRepresentationInvariants_Tests
{
    [Theory(DisplayName = "不变量：RuleId 的字符串表示是稳定规范格式")]
    [InlineData(907, 3, "ADR-907_3")]
    [InlineData(1, 1, "ADR-001_1")]
    [InlineData(900, 2, "ADR-900_2")]
    public void RuleId_ToString_Should_Follow_Spec(int adr, int rule, string expected)
    {
        var id = ArchitectureRuleId.Rule(adr, rule);

        id.ToString().Should().Be(expected, 
            $"RuleId({adr}, {rule}) 的字符串表示应为 '{expected}'");
    }

    [Theory(DisplayName = "不变量：ClauseId 的字符串表示是稳定规范格式")]
    [InlineData(907, 3, 1, "ADR-907_3_1")]
    [InlineData(900, 1, 1, "ADR-900_1_1")]
    [InlineData(1, 1, 1, "ADR-001_1_1")]
    public void ClauseId_ToString_Should_Follow_Spec(int adr, int rule, int clause, string expected)
    {
        var id = ArchitectureRuleId.Clause(adr, rule, clause);

        id.ToString().Should().Be(expected, 
            $"ClauseId({adr}, {rule}, {clause}) 的字符串表示应为 '{expected}'");
    }
}