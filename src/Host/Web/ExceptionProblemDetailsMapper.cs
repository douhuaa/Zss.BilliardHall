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
        // 验证异常：从 ErrorRegistry 获取描述
        if (ex is PlatformValidationException ve)
            return MapValidation(ve, requestPath);

        // InfrastructureException：技术异常，直接使用其携带的 HttpStatusCode 映射
        if (ex is InfrastructureException ie)
            return MapInfrastructure(ie, requestPath);

        // 获取错误码
        var code = ex switch
        {
            DomainError d => d.Code,
            DomainException de => de.ErrorCode,
            _ => CommonErrorCodes.Unknown
        };

        ErrorDescriptor descriptor;
        try
        {
            descriptor = ErrorRegistry.Get(code);
        }
        catch (KeyNotFoundException)
        {
            // DomainException 或 DomainError 的 ErrorCode 未在 ErrorRegistry 中注册，
            // 降级为 Unknown 以保证生产环境稳健性。
            // 若需在开发阶段及早发现，可在启动时验证所有已知异常子类的错误码均已注册。
            descriptor = ErrorRegistry.Get(CommonErrorCodes.Unknown);
        }

        return new ProblemDetails
        {
            Type = descriptor.ProblemType,
            Title = descriptor.Title,
            Status = descriptor.HttpStatus,
            Detail = BuildDetail(ex, descriptor, includeExceptionDetail),
            Instance = requestPath,
            Extensions =
            {
                ["errorCode"] = descriptor.Code
            }
        };
    }

    private static string? BuildDetail(Exception ex, ErrorDescriptor descriptor, bool includeExceptionDetail)
    {
        if (includeExceptionDetail)
            return ex.ToString();

        // 对于未知/降级的错误，不暴露原始异常消息（安全性）
        if (descriptor.Code == CommonErrorCodes.Unknown)
            return "发生未处理异常，请联系管理员。";

        return ex.Message != descriptor.Title ? ex.Message : null;
    }

    private static ProblemDetails MapInfrastructure(InfrastructureException ex, string? requestPath)
    {
        var status = ex.HttpStatusCode ?? StatusCodes.Status503ServiceUnavailable;
        return new ProblemDetails
        {
            Type = ProblemType.FromStatusCode(status),
            Title = "服务暂时不可用",
            Status = status,
            Detail = ex.Message,
            Instance = requestPath
        };
    }

    private static ValidationProblemDetails MapValidation(PlatformValidationException ex, string? requestPath)
    {
        var descriptor = ErrorRegistry.Get(CommonErrorCodes.Validation);
        return new ValidationProblemDetails(BuildValidationErrors(ex))
        {
            Type = descriptor.ProblemType,
            Title = descriptor.Title,
            Status = descriptor.HttpStatus,
            Detail = "一个或多个验证错误发生。",
            Instance = requestPath,
            Extensions = { ["errorCode"] = CommonErrorCodes.Validation }
        };
    }

    private static Dictionary<string, string[]> BuildValidationErrors(PlatformValidationException ex)
        => ex.Errors.Count > 0
            ? ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<string, string[]> { ["_"] = [ex.Message] };
}
