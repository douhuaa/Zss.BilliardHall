namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

public sealed class ArchitectureRuleIdParsingInvariants_Tests
{
    [Theory(DisplayName = "不变量：合法字符串必须可被解析")]
    [InlineData("ADR-907_3")]
    [InlineData("ADR-907_3_1")]
    [InlineData("907_3")]
    [InlineData("907_3_1")]
    public void Parse_Should_Accept_Valid_Formats(string input)
    {
        var id = ArchitectureRuleId.Parse(input);

        id.Should().NotBeNull();
    }

    [Theory(DisplayName = "不变量：非法格式必须被拒绝")]
    [InlineData("ADR-")]
    [InlineData("ADR-907")]
    [InlineData("ADR-907__3")]
    [InlineData("ADR-907_3_")]
    [InlineData("ADR--3")]
    public void Parse_Should_Reject_Invalid_Formats(string input)
    {
        Action act = () => ArchitectureRuleId.Parse(input);

        act.Should().Throw<ArgumentException>();
    }
}