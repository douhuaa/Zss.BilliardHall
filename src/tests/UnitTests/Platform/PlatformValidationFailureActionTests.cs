using FluentAssertions;
using FluentValidation.Results;
using Zss.BilliardHall.Platform.Infrastructure;
using PlatformValidationException = Zss.BilliardHall.Platform.Exceptions.ValidationException;

namespace Zss.BilliardHall.Tests.UnitTests.Platform;

public sealed class PlatformValidationFailureActionTests
{
    [Fact]
    public void Throw_WithSingleFailure_ThrowsPlatformValidationException()
    {
        // Arrange
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "姓名不能为空")
        };

        // Act
        var act = () => sut.Throw(new TestCommand(), failures);

        // Assert
        act.Should().Throw<PlatformValidationException>()
            .Which.Errors.Should().ContainKey("Name")
            .WhoseValue.Should().Contain("姓名不能为空");
    }

    [Fact]
    public void Throw_WithMultipleFailuresSameProperty_GroupsByProperty()
    {
        // Arrange
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure>
        {
            new("Email", "邮箱不能为空"),
            new("Email", "邮箱格式不正确")
        };

        // Act
        var act = () => sut.Throw(new TestCommand(), failures);

        // Assert
        var ex = act.Should().Throw<PlatformValidationException>().Which;
        ex.Errors.Should().ContainKey("Email");
        ex.Errors["Email"].Should().HaveCount(2);
        ex.Errors["Email"].Should().Contain("邮箱不能为空");
        ex.Errors["Email"].Should().Contain("邮箱格式不正确");
    }

    [Fact]
    public void Throw_WithMultipleProperties_ContainsAllProperties()
    {
        // Arrange
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "姓名不能为空"),
            new("Email", "邮箱不能为空"),
            new("Phone", "手机号格式不正确")
        };

        // Act
        var act = () => sut.Throw(new TestCommand(), failures);

        // Assert
        var ex = act.Should().Throw<PlatformValidationException>().Which;
        ex.Errors.Should().HaveCount(3);
        ex.Errors.Should().ContainKey("Name");
        ex.Errors.Should().ContainKey("Email");
        ex.Errors.Should().ContainKey("Phone");
    }

    [Fact]
    public void Throw_SetsCorrectMessage()
    {
        // Arrange
        var sut = new PlatformValidationFailureAction<TestCommand>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "姓名不能为空")
        };

        // Act
        var act = () => sut.Throw(new TestCommand(), failures);

        // Assert
        act.Should().Throw<PlatformValidationException>()
            .Which.Message.Should().Be("验证失败，请检查输入数据。");
    }

    private sealed record TestCommand;
}
