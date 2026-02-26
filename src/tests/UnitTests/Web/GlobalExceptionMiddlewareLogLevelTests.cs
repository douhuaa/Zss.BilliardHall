using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Zss.BilliardHall.Host.Web;
using Zss.BilliardHall.Platform.Contracts;
using Zss.BilliardHall.Platform.Errors;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Tests.UnitTests.Web;

/// <summary>
/// GlobalExceptionMiddleware 日志级别策略测试：
/// 验证通过 errorCode 查 descriptor.LogLevel 决定日志级别，
/// 并始终传递 original Exception。
/// </summary>
public class GlobalExceptionMiddlewareLogLevelTests
{
    private sealed class TestDomainException(string errorCode, string message)
        : DomainException(errorCode, message);

    private static (GlobalExceptionMiddleware, Mock<ILogger<GlobalExceptionMiddleware>>) CreateSut(
        string errorCode,
        int httpStatus,
        LogLevel expectedLogLevel)
    {
        var registry = new ErrorRegistry();
        new CommonErrorRegistrar().Register(registry);
        registry.Register(new ErrorDescriptor(errorCode, httpStatus, "测试错误", expectedLogLevel));

        var mapperMock = new Mock<IExceptionProblemDetailsMapper>();
        mapperMock
            .Setup(m => m.Map(It.IsAny<Exception>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(() =>
            {
                var pd = new ProblemDetails { Status = httpStatus };
                pd.Extensions["errorCode"] = errorCode;
                return pd;
            });

        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionMiddleware(
            next: _ => throw new TestDomainException(errorCode, "触发测试异常"),
            logger: loggerMock.Object,
            environment: envMock.Object,
            mapper: mapperMock.Object,
            translators: [],
            errorRegistry: registry);

        return (middleware, loggerMock);
    }

    [Theory]
    [InlineData("COMMON_VALIDATION_FAILED", 400, LogLevel.Warning)]
    [InlineData("COMMON_UNKNOWN_ERROR", 500, LogLevel.Error)]
    public async Task InvokeAsync_LogsAtCorrectLevelFromDescriptor(string errorCode, int httpStatus, LogLevel expectedLogLevel)
    {
        var (middleware, loggerMock) = CreateSut(errorCode, httpStatus, expectedLogLevel);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        loggerMock.Verify(
            l => l.Log(
                expectedLogLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            $"应以 {expectedLogLevel} 级别记录日志");
    }

    [Fact]
    public async Task InvokeAsync_AlwaysPassesOriginalException_ToLogger()
    {
        var errorCode = "COMMON_UNKNOWN_ERROR";
        var registry = new ErrorRegistry();
        new CommonErrorRegistrar().Register(registry);

        var originalException = new InvalidOperationException("原始异常");
        Exception? capturedLogException = null;

        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        loggerMock
            .Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>((_, _, _, ex, _) =>
                capturedLogException = ex);

        var mapperMock = new Mock<IExceptionProblemDetailsMapper>();
        mapperMock
            .Setup(m => m.Map(It.IsAny<Exception>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(() =>
            {
                var pd = new ProblemDetails { Status = 500 };
                pd.Extensions["errorCode"] = errorCode;
                return pd;
            });

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionMiddleware(
            next: _ => throw originalException,
            logger: loggerMock.Object,
            environment: envMock.Object,
            mapper: mapperMock.Object,
            translators: [],
            errorRegistry: registry);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        capturedLogException.Should().BeSameAs(originalException, "日志应包含原始异常对象（ADR-350）");
    }
}
