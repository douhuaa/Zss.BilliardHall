using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zss.BilliardHall.Host.Web;
using Zss.BilliardHall.Platform.Errors;
using Zss.BilliardHall.Platform.Exceptions;
using Zss.BilliardHall.Tests.SharedTestHelpers;

namespace Zss.BilliardHall.Tests.UnitTests.Web;

[Collection(ErrorRegistryCollection.Name)]
public class ExceptionProblemDetailsMapperTests : IClassFixture<ErrorRegistryFixture>
{
    private readonly ExceptionProblemDetailsMapper _sut = new();

    // 用于测试的具体 DomainException 子类（DomainException 是抽象类）
    private sealed class TestDomainException(string errorCode, string message)
        : DomainException(errorCode, message);

    public ExceptionProblemDetailsMapperTests(ErrorRegistryFixture registry)
    {
        // 注册平台通用错误码（测试所需）
        registry.TryRegister(new ErrorDescriptor(
            CommonErrorCodes.Unknown, "未知错误",
            StatusCodes.Status500InternalServerError,
            ProblemType.FromStatusCode(500), Microsoft.Extensions.Logging.LogLevel.Error));
        registry.TryRegister(new ErrorDescriptor(
            CommonErrorCodes.Validation, "验证失败",
            StatusCodes.Status400BadRequest,
            ProblemType.Validation, Microsoft.Extensions.Logging.LogLevel.Warning));

        // 注册测试用领域错误码（title 有意与 DomainException.message 不同，验证 Detail 逻辑）
        registry.TryRegister(new ErrorDescriptor(
            "TABLE_NOT_FOUND", "台球桌资源未找到",
            StatusCodes.Status409Conflict,
            ProblemType.Domain, Microsoft.Extensions.Logging.LogLevel.Information));
        registry.TryRegister(new ErrorDescriptor(
            "BOOKING_CONFLICT", "预订存在冲突",
            StatusCodes.Status409Conflict,
            ProblemType.Domain, Microsoft.Extensions.Logging.LogLevel.Information));
    }

    #region DomainException -> 409

    [Theory]
    [InlineData("TABLE_NOT_FOUND", "台球桌不存在", "/api/tables/1")]
    [InlineData("BOOKING_CONFLICT", "预订时间冲突", "/api/bookings")]
    public void Map_DomainException_Returns409WithErrorCode(string errorCode, string message, string path)
    {
        var ex = new TestDomainException(errorCode, message);

        var result = _sut.Map(ex, path, includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status409Conflict);
        result.Type.Should().Be(ProblemType.Domain);
        result.Detail.Should().Be(message);
        result.Instance.Should().Be(path);
        result.Extensions["errorCode"].Should().Be(errorCode);
    }

    #endregion

    #region InfrastructureException

    [Fact]
    public void Map_InfrastructureException_DefaultsTo503()
    {
        var ex = new InfrastructureException("Database", "数据库连接失败");

        var result = _sut.Map(ex, "/api/test", includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status503ServiceUnavailable);
        result.Type.Should().Be(ProblemType.FromStatusCode(503));
    }

    [Fact]
    public void Map_InfrastructureException_WithHttpStatusCode_ReturnsSpecifiedStatus()
    {
        var ex = new InfrastructureException("Gateway", "网关错误") { HttpStatusCode = StatusCodes.Status502BadGateway };

        var result = _sut.Map(ex, "/api/test", includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status502BadGateway);
        result.Type.Should().Be(ProblemType.FromStatusCode(502));
    }

    #endregion

    #region Platform.ValidationException

    [Fact]
    public void Map_PlatformValidationException_WithErrors_ReturnsValidationProblemDetails()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["姓名不能为空"],
            ["Phone"] = ["手机号格式不正确"]
        };
        var ex = new Zss.BilliardHall.Platform.Exceptions.ValidationException("验证失败", errors);

        var result = _sut.Map(ex, "/api/members", includeExceptionDetail: false);

        result.Should().BeOfType<ValidationProblemDetails>();
        result.Status.Should().Be(StatusCodes.Status400BadRequest);
        result.Type.Should().Be(ProblemType.Validation);
        result.Title.Should().Be("验证失败");
        result.Detail.Should().Be("一个或多个验证错误发生。");
        var vpd = (ValidationProblemDetails)result;
        vpd.Errors.Should().ContainKey("Name");
        vpd.Errors.Should().ContainKey("Phone");
    }

    [Fact]
    public void Map_PlatformValidationException_EmptyErrors_FallsToUnderscoreKey()
    {
        var ex = new Zss.BilliardHall.Platform.Exceptions.ValidationException("总体验证失败");

        var result = _sut.Map(ex, "/api/members", includeExceptionDetail: false);

        result.Should().BeOfType<ValidationProblemDetails>();
        var vpd = (ValidationProblemDetails)result;
        vpd.Errors.Should().ContainKey("_");
        vpd.Errors["_"].Should().Contain("总体验证失败");
    }

    #endregion

    #region Unknown Exception

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Map_UnknownException_RespectIncludeExceptionDetail(bool includeDetail)
    {
        var ex = new InvalidOperationException("意外操作异常");

        var result = _sut.Map(ex, "/api/test", includeDetail);

        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
        result.Type.Should().Be(ProblemType.FromStatusCode(500));

        if (includeDetail)
            result.Detail.Should().Contain("InvalidOperationException");
        else
            result.Detail.Should().Be("发生未处理异常，请联系管理员。");
    }

    #endregion
}
