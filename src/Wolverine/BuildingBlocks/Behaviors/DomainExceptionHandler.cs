using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.BuildingBlocks.Core;

namespace Zss.BilliardHall.BuildingBlocks.Behaviors;

/// <summary>
/// Wolverine 异常处理中间件：在应用边界统一转换异常
/// Wolverine exception handling middleware: unified exception conversion at application boundary
/// </summary>
public static class DomainExceptionHandler
{
    /// <summary>
    /// 将 ModuleDomainException 转换为 Result 和 HTTP 状态码
    /// Convert ModuleDomainException to Result and HTTP status code
    /// </summary>
    public static (Result Result, int StatusCode) ToResult(
        ModuleDomainException exception,
        ILogger logger)
    {
        var descriptor = exception.ErrorDescriptor;

        // 根据错误类别选择日志级别
        var logLevel = descriptor.Category switch
        {
            ErrorCategory.NotFound => LogLevel.Warning,
            ErrorCategory.Validation => LogLevel.Warning,
            ErrorCategory.Business => LogLevel.Information,
            ErrorCategory.Conflict => LogLevel.Warning,
            ErrorCategory.Forbidden => LogLevel.Warning,
            ErrorCategory.InvalidStatus => LogLevel.Warning,
            _ => LogLevel.Error
        };

        logger.Log(
            logLevel,
            exception,
            "领域异常: {Module}:{Category}, 错误码: {Code}, 消息: {Message}",
            descriptor.Module,
            descriptor.Category,
            descriptor.Code,
            descriptor.FormatMessage()
        );

        // 根据错误类别映射 HTTP 状态码
        var statusCode = descriptor.Category switch
        {
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.Business => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCategory.InvalidStatus => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var result = Result.Fail(descriptor.FormatMessage(), descriptor.Code);

        return (result, statusCode);
    }

    /// <summary>
    /// 将 DomainResult 转换为 Result 和 HTTP 状态码
    /// Convert DomainResult to Result and HTTP status code
    /// </summary>
    public static (Result Result, int StatusCode) ToResult(
        DomainResult domainResult,
        ILogger logger)
    {
        if (domainResult.IsSuccess)
        {
            return (Result.Success(), StatusCodes.Status200OK);
        }

        // 优先使用 ErrorDescriptor
        if (domainResult.ErrorDescriptor != null)
        {
            var descriptor = domainResult.ErrorDescriptor;

            logger.LogWarning(
                "领域规则违反: {Module}:{Category}, 错误码: {Code}, 消息: {Message}",
                descriptor.Module,
                descriptor.Category,
                descriptor.Code,
                descriptor.FormatMessage()
            );

            // 根据错误类别映射 HTTP 状态码
            var statusCode = descriptor.Category switch
            {
                ErrorCategory.NotFound => StatusCodes.Status404NotFound,
                ErrorCategory.Validation => StatusCodes.Status400BadRequest,
                ErrorCategory.Business => StatusCodes.Status422UnprocessableEntity,
                ErrorCategory.Conflict => StatusCodes.Status409Conflict,
                ErrorCategory.Forbidden => StatusCodes.Status403Forbidden,
                ErrorCategory.InvalidStatus => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            var result = Result.Fail(descriptor.FormatMessage(), descriptor.Code);
            return (result, statusCode);
        }

        // 向后兼容：处理旧版 ErrorCode
#pragma warning disable CS0618 // Type or member is obsolete
        if (domainResult.Error != null)
        {
            var errorCode = domainResult.Error.Code;

            logger.LogWarning(
                "领域规则违反 (旧版): 错误码: {Code}",
                errorCode
            );

            // 简单的错误码到消息的映射（旧版兼容）
            var message = errorCode switch
            {
                "Member.InvalidTopUpAmount" => "充值金额必须大于0",
                "Member.InvalidDeductAmount" => "扣减金额必须大于0",
                "Member.InsufficientBalance" => "余额不足",
                "Member.InvalidAwardPoints" => "赠送积分必须大于0",
                _ => "操作失败"
            };

            var result = Result.Fail(message, errorCode);
            return (result, StatusCodes.Status400BadRequest);
        }
#pragma warning restore CS0618

        // 默认处理
        logger.LogWarning("未知的领域规则违反");
        return (Result.Fail("操作失败"), StatusCodes.Status400BadRequest);
    }
}
