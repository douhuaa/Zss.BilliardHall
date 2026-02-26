using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.Platform.Errors;

namespace Zss.BilliardHall.Tests.UnitTests.Platform;

public class ErrorRegistryTests
{
    private static ErrorRegistry CreateRegistryWithCommon()
    {
        var registry = new ErrorRegistry();
        new CommonErrorRegistrar().Register(registry);
        return registry;
    }

    #region Register + Find

    [Theory]
    [InlineData("COMMON_VALIDATION_FAILED", 400, "验证失败")]
    [InlineData("COMMON_UNKNOWN_ERROR", 500, "服务器内部错误")]
    public void CommonErrorRegistrar_RegistersExpectedDescriptors(string errorCode, int expectedStatus, string expectedTitle)
    {
        var registry = CreateRegistryWithCommon();

        var descriptor = registry.Find(errorCode);

        descriptor.Should().NotBeNull();
        descriptor!.ErrorCode.Should().Be(errorCode);
        descriptor.HttpStatusCode.Should().Be(expectedStatus);
        descriptor.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void Find_UnregisteredCode_ReturnsNull()
    {
        var registry = new ErrorRegistry();

        registry.Find("NONEXISTENT_CODE").Should().BeNull();
    }

    [Fact]
    public void Register_OverwritesExistingCode()
    {
        var registry = new ErrorRegistry();
        registry.Register(new ErrorDescriptor("MY_CODE", 400, "旧标题"));
        registry.Register(new ErrorDescriptor("MY_CODE", 422, "新标题"));

        var descriptor = registry.Find("MY_CODE");
        descriptor!.HttpStatusCode.Should().Be(422);
        descriptor.Title.Should().Be("新标题");
    }

    #endregion

    #region GetOrFallback（未注册 errorCode 的 fallback 行为）

    [Fact]
    public void GetOrFallback_UnregisteredCode_ReturnsFallbackDescriptor()
    {
        var registry = CreateRegistryWithCommon();

        var descriptor = registry.GetOrFallback("NONEXISTENT_CODE");

        descriptor.ErrorCode.Should().Be(CommonErrorCodes.UnknownError);
        descriptor.HttpStatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void GetOrFallback_RegisteredCode_ReturnsCorrectDescriptor()
    {
        var registry = CreateRegistryWithCommon();
        registry.Register(new ErrorDescriptor("MY_DOMAIN_ERROR", 409, "领域错误", LogLevel.Warning));

        var descriptor = registry.GetOrFallback("MY_DOMAIN_ERROR");

        descriptor.ErrorCode.Should().Be("MY_DOMAIN_ERROR");
        descriptor.HttpStatusCode.Should().Be(409);
    }

    #endregion

    #region CommonErrorCodes 常量

    [Theory]
    [InlineData("COMMON_VALIDATION_FAILED")]
    public void CommonErrorCodes_ValidationFailed_IsExpectedValue(string expected)
    {
        CommonErrorCodes.ValidationFailed.Should().Be(expected);
    }

    [Theory]
    [InlineData("COMMON_UNKNOWN_ERROR")]
    public void CommonErrorCodes_UnknownError_IsExpectedValue(string expected)
    {
        CommonErrorCodes.UnknownError.Should().Be(expected);
    }

    #endregion

    #region LogLevel（日志级别）

    [Theory]
    [InlineData("COMMON_VALIDATION_FAILED", LogLevel.Warning)]
    [InlineData("COMMON_UNKNOWN_ERROR", LogLevel.Error)]
    public void CommonDescriptors_HaveExpectedLogLevel(string errorCode, LogLevel expectedLogLevel)
    {
        var registry = CreateRegistryWithCommon();

        var descriptor = registry.Find(errorCode);

        descriptor!.LogLevel.Should().Be(expectedLogLevel);
    }

    #endregion
}
