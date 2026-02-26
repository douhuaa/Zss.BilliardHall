using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.Host.Web;
using Zss.BilliardHall.Platform.Errors;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Tests.UnitTests.Web;

public class ExceptionProblemDetailsMapperTests
{
    // 用于测试的具体 DomainException 子类（DomainException 是抽象类）
    private sealed class TestDomainException(string errorCode, string message)
        : DomainException(errorCode, message);

    private static ExceptionProblemDetailsMapper CreateSut(Action<ErrorRegistry>? configure = null)
    {
        var registry = new ErrorRegistry();

        // 始终注册公共错误码
        new CommonErrorRegistrar().Register(registry);

        // 注册领域错误码（409）
        registry.Register(new ErrorDescriptor("TABLE_NOT_FOUND", StatusCodes.Status409Conflict, "业务规则违反", LogLevel.Warning));
        registry.Register(new ErrorDescriptor("BOOKING_CONFLICT", StatusCodes.Status409Conflict, "业务规则违反", LogLevel.Warning));
        registry.Register(new ErrorDescriptor("MEMBER_EMAIL_EXISTS", StatusCodes.Status409Conflict, "业务规则违反", LogLevel.Warning));
        registry.Register(new ErrorDescriptor("INFRA_DB_ERROR", StatusCodes.Status503ServiceUnavailable, "服务暂时不可用", LogLevel.Error));

        configure?.Invoke(registry);
        return new ExceptionProblemDetailsMapper(registry);
    }

    #region DomainException -> 使用 registry 描述符

    [Theory]
    [InlineData("TABLE_NOT_FOUND", "台球桌不存在", "/api/tables/1")]
    [InlineData("BOOKING_CONFLICT", "预订时间冲突", "/api/bookings")]
    public void Map_DomainException_ReturnsRegistryDescriptorStatus(string errorCode, string message, string path)
    {
        var sut = CreateSut();
        var ex = new TestDomainException(errorCode, message);

        var result = sut.Map(ex, path, includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status409Conflict);
        result.Type.Should().Be(ProblemType.Domain);
        result.Detail.Should().Be(message);
        result.Instance.Should().Be(path);
        result.Extensions["errorCode"].Should().Be(errorCode);
    }

    #endregion

    #region InfrastructureException

    [Fact]
    public void Map_InfrastructureException_UsesRegistryDescriptorStatus()
    {
        var sut = CreateSut();
        var ex = new InfrastructureException("Database", "数据库连接失败") { ErrorCode = "INFRA_DB_ERROR" };

        var result = sut.Map(ex, "/api/test", includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status503ServiceUnavailable);
        result.Extensions["errorCode"].Should().Be("INFRA_DB_ERROR");
    }

    [Fact]
    public void Map_InfrastructureException_WithHttpStatusCode_OverridesDescriptor()
    {
        var sut = CreateSut();
        var ex = new InfrastructureException("Gateway", "网关错误")
        {
            ErrorCode = "INFRA_DB_ERROR",
            HttpStatusCode = StatusCodes.Status502BadGateway
        };

        var result = sut.Map(ex, "/api/test", includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status502BadGateway);
    }

    [Fact]
    public void Map_InfrastructureException_DefaultErrorCode_IsUnknownError()
    {
        var ex = new InfrastructureException("Database", "数据库连接失败");

        ex.ErrorCode.Should().Be(CommonErrorCodes.UnknownError);
    }

    #endregion

    #region Platform.ValidationException

    [Fact]
    public void Map_PlatformValidationException_WithErrors_ReturnsValidationProblemDetailsWithErrorCode()
    {
        var sut = CreateSut();
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["姓名不能为空"],
            ["Phone"] = ["手机号格式不正确"]
        };
        var ex = new Zss.BilliardHall.Platform.Exceptions.ValidationException("验证失败", errors);

        var result = sut.Map(ex, "/api/members", includeExceptionDetail: false);

        result.Should().BeOfType<ValidationProblemDetails>();
        result.Status.Should().Be(StatusCodes.Status400BadRequest);
        result.Type.Should().Be(ProblemType.Validation);
        result.Title.Should().Be("验证失败");
        result.Detail.Should().Be("一个或多个验证错误发生。");
        result.Extensions["errorCode"].Should().Be(CommonErrorCodes.ValidationFailed);
        var vpd = (ValidationProblemDetails)result;
        vpd.Errors.Should().ContainKey("Name");
        vpd.Errors.Should().ContainKey("Phone");
    }

    [Fact]
    public void Map_PlatformValidationException_EmptyErrors_FallsToUnderscoreKey()
    {
        var sut = CreateSut();
        var ex = new Zss.BilliardHall.Platform.Exceptions.ValidationException("总体验证失败");

        var result = sut.Map(ex, "/api/members", includeExceptionDetail: false);

        result.Should().BeOfType<ValidationProblemDetails>();
        var vpd = (ValidationProblemDetails)result;
        vpd.Errors.Should().ContainKey("_");
        vpd.Errors["_"].Should().Contain("总体验证失败");
    }

    [Fact]
    public void ValidationException_ErrorCode_IsAlwaysValidationFailed()
    {
        var ex = new Zss.BilliardHall.Platform.Exceptions.ValidationException("验证失败");

        ex.ErrorCode.Should().Be(CommonErrorCodes.ValidationFailed);
    }

    #endregion

    #region Unknown Exception（未注册 errorCode 的 fallback 行为）

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Map_UnknownException_FallsBackToUnknownErrorDescriptor(bool includeDetail)
    {
        var sut = CreateSut();
        var ex = new InvalidOperationException("意外操作异常");

        var result = sut.Map(ex, "/api/test", includeDetail);

        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
        result.Extensions["errorCode"].Should().Be(CommonErrorCodes.UnknownError);

        if (includeDetail)
            result.Detail.Should().Contain("InvalidOperationException");
        else
            result.Detail.Should().Be("意外操作异常");
    }

    [Fact]
    public void Map_UnregisteredDomainErrorCode_FallsBackToUnknownError()
    {
        var sut = CreateSut(); // 没有注册 UNREGISTERED_CODE
        var ex = new TestDomainException("UNREGISTERED_CODE", "未注册的错误");

        var result = sut.Map(ex, "/api/test", includeExceptionDetail: false);

        // fallback 到 COMMON_UNKNOWN_ERROR descriptor
        result.Extensions["errorCode"].Should().Be(CommonErrorCodes.UnknownError);
        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region IHasErrorCode 接口合规性

    [Fact]
    public void DomainException_ImplementsIHasErrorCode()
    {
        var ex = new TestDomainException("TABLE_NOT_FOUND", "台球桌不存在");

        ex.Should().BeAssignableTo<IHasErrorCode>();
        ((IHasErrorCode)ex).ErrorCode.Should().Be("TABLE_NOT_FOUND");
    }

    [Fact]
    public void ValidationException_ImplementsIHasErrorCode()
    {
        var ex = new Zss.BilliardHall.Platform.Exceptions.ValidationException("验证失败");

        ex.Should().BeAssignableTo<IHasErrorCode>();
        ((IHasErrorCode)ex).ErrorCode.Should().Be(CommonErrorCodes.ValidationFailed);
    }

    [Fact]
    public void InfrastructureException_ImplementsIHasErrorCode()
    {
        var ex = new InfrastructureException("Database", "数据库连接失败");

        ex.Should().BeAssignableTo<IHasErrorCode>();
    }

    #endregion
}
