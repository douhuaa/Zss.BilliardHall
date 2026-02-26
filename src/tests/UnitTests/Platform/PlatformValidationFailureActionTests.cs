using FluentValidation.Results;
using Zss.BilliardHall.Platform.Errors;
using Zss.BilliardHall.Platform.Infrastructure.Wolverine;
using PlatformValidationException = Zss.BilliardHall.Platform.Exceptions.ValidationException;

namespace Zss.BilliardHall.Tests.UnitTests.Platform;

public sealed class PlatformValidationFailureActionTests
{
    private sealed record TestCommand;

    #region 单字段单错误

    [Theory]
    [InlineData("Name", "姓名不能为空")]
    [InlineData("Email", "邮箱格式不正确")]
    [InlineData("Phone", "手机号不能为空")]
    public void Throw_SingleFieldSingleError_ErrorIsMappedToField(string field, string error)
    {
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure> { new(field, error) };

        var act = () => sut.Throw(new TestCommand(), failures);

        act.Should().Throw<PlatformValidationException>()
            .Which.Errors.Should().ContainKey(field)
            .WhoseValue.Should().Contain(error);
    }

    #endregion

    #region 单字段多错误（聚合）

    [Fact]
    public void Throw_SingleFieldMultipleErrors_AggregatesUnderSameKey()
    {
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure>
        {
            new("Email", "邮箱不能为空"),
            new("Email", "邮箱格式不正确")
        };

        var act = () => sut.Throw(new TestCommand(), failures);

        var ex = act.Should().Throw<PlatformValidationException>().Which;
        ex.Errors.Should().ContainKey("Email");
        ex.Errors["Email"].Should().HaveCount(2)
            .And.Contain("邮箱不能为空")
            .And.Contain("邮箱格式不正确");
    }

    #endregion

    #region 多字段

    [Fact]
    public void Throw_MultipleFields_ContainsAllFieldKeys()
    {
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "姓名不能为空"),
            new("Email", "邮箱不能为空"),
            new("Phone", "手机号格式不正确")
        };

        var act = () => sut.Throw(new TestCommand(), failures);

        var ex = act.Should().Throw<PlatformValidationException>().Which;
        ex.Errors.Should().HaveCount(3)
            .And.ContainKey("Name")
            .And.ContainKey("Email")
            .And.ContainKey("Phone");
    }

    #endregion

    #region message 常量一致性

    [Theory]
    [InlineData("Name", "姓名不能为空")]
    [InlineData("Email", "邮箱格式不正确")]
    public void Throw_AlwaysUsesValidationFailureConstantAsMessage(string field, string error)
    {
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure> { new(field, error) };

        var act = () => sut.Throw(new TestCommand(), failures);

        act.Should().Throw<PlatformValidationException>()
            .Which.Message.Should().Be(PlatformErrorMessages.ValidationFailure);
    }

    #endregion
}
