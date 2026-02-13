namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// ArchitectureRuleId 解析不变量测试
/// 验证 RuleId 字符串解析的边界和约束
/// 
/// 核心不变量：
/// - 合法格式应该被正确解析
/// - 非法格式应该抛出明确的异常
/// - 解析失败应该提供清晰的错误信息
/// </summary>
public sealed class ArchitectureRuleIdParsingInvariants_Tests
{
    [Theory(DisplayName = "不变量：合法字符串必须可被解析")]
    [InlineData("ADR-907_3")]
    [InlineData("ADR-907_3_1")]
    [InlineData("907_3")]
    [InlineData("907_3_1")]
    public void Parse_Should_Accept_Valid_Formats(string input)
    {
        var act = () => ArchitectureRuleId.Parse(input);

        act.Should().NotThrow($"'{input}' 是合法格式，应该成功解析");
        
        var id = ArchitectureRuleId.Parse(input);
        id.Should().NotBe(default(ArchitectureRuleId), "解析结果应该是有效的 RuleId");
    }

    [Theory(DisplayName = "不变量：非法格式必须被拒绝")]
    [InlineData("ADR-")]
    [InlineData("ADR-907")]
    [InlineData("ADR-907__3")]
    [InlineData("ADR-907_3_")]
    [InlineData("ADR--3")]
    public void Parse_Should_Reject_Invalid_Formats(string input)
    {
        var act = () => ArchitectureRuleId.Parse(input);

        act.Should().Throw<ArgumentException>(
            $"'{input}' 是非法格式，应该抛出 ArgumentException")
            .WithMessage("*无效的 RuleId 格式*", "错误消息应该明确指出格式错误");
    }
}