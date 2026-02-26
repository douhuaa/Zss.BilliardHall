using Microsoft.AspNetCore.Mvc;
using Zss.BilliardHall.Platform.Errors;
using Zss.BilliardHall.Platform.Exceptions;
using PlatformValidationException = Zss.BilliardHall.Platform.Exceptions.ValidationException;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 将异常映射为 ProblemDetails 的抽象契约
/// </summary>
public interface IExceptionProblemDetailsMapper
{
    /// <summary>
    /// 将异常映射为对应的 ProblemDetails
    /// </summary>
    /// <param name="ex">待映射的异常</param>
    /// <param name="requestPath">请求路径（用于 Instance 字段）</param>
    /// <param name="includeExceptionDetail">是否在 Detail 中包含完整异常信息（仅 Development 环境传 true）</param>
    ProblemDetails Map(Exception ex, string? requestPath, bool includeExceptionDetail);
}

/// <summary>
/// 将异常映射为 ProblemDetails，纯映射逻辑，不依赖 HttpContext
/// </summary>
public sealed class ExceptionProblemDetailsMapper : IExceptionProblemDetailsMapper
{
    public ProblemDetails Map(Exception ex, string? requestPath, bool includeExceptionDetail)
    {
        // 如果是验证异常，保持原有的 special handling
        if (ex is PlatformValidationException ve)
        {
            return MapValidation(ve, requestPath);
        }

        // 获取错误码
        var code = ex switch
        {
            DomainError d => d.Code,
            DomainException de => de.ErrorCode, // 兼容旧代码，如果旧代码 ErrorCode 未注册会报错，这是预期的架构测试行为
            _ => CommonErrorCodes.Unknown
        };

        // 从注册中心获取描述
        // 如果未注册，这会抛出 KeyNotFoundException，这在生产环境可能不好，但在开发阶段能强制注册
        // 为了稳健性，这里可以 fallback
        ErrorDescriptor descriptor;
        try
        {
            descriptor = ErrorRegistry.Get(code);
        }
        catch (KeyNotFoundException)
        {
            // 如果是未知异常导致的 Common.Unknown，应该已注册。
            // 如果是 DomainException 的 ErrorCode 未注册，这里降级为 Unknown
            descriptor = ErrorRegistry.Get(CommonErrorCodes.Unknown);
        }

        return new ProblemDetails
        {
            Type = descriptor.ProblemType,
            Title = descriptor.Title,
            Status = descriptor.HttpStatus,
            Detail = includeExceptionDetail ? ex.ToString() : (ex.Message != descriptor.Title ? ex.Message : null),
            Instance = requestPath,
            Extensions =
            {
                ["errorCode"] = descriptor.Code
            }
        };
    }

    private static ValidationProblemDetails MapValidation(PlatformValidationException ex, string? requestPath)
        => new(BuildValidationErrors(ex))
        {
            Type = "https://api.zss.com/problems/common/validation", // Hardcoded or from registry? keeping simple
            Title = "验证失败",
            Status = StatusCodes.Status400BadRequest,
            Detail = "一个或多个验证错误发生。",
            Instance = requestPath,
            Extensions = { ["errorCode"] = CommonErrorCodes.Validation }
        };

    private static Dictionary<string, string[]> BuildValidationErrors(PlatformValidationException ex)
        => ex.Errors.Count > 0
            ? ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<string, string[]> { ["_"] = [ex.Message] };
}
