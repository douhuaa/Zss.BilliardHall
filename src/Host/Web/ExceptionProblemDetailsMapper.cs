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
/// 将异常映射为 ProblemDetails。
/// 通过 IErrorRegistry 查找错误码描述符驱动语义，不再按异常类型硬编码映射逻辑。
/// ValidationException 特殊处理，返回 ValidationProblemDetails 并携带 errorCode 和 Errors 字段。
/// 未注册 errorCode 时 fallback 到 COMMON_UNKNOWN_ERROR 描述符（避免二次 500）。
/// </summary>
public sealed class ExceptionProblemDetailsMapper(IErrorRegistry registry) : IExceptionProblemDetailsMapper
{
    public ProblemDetails Map(Exception ex, string? requestPath, bool includeExceptionDetail)
    {
        if (ex is PlatformValidationException ve)
            return MapValidation(ve, requestPath);

        var errorCode = ExtractErrorCode(ex);
        var descriptor = registry.GetOrFallback(errorCode);

        return BuildProblemDetails(ex, requestPath, descriptor, includeExceptionDetail);
    }

    private static string ExtractErrorCode(Exception ex)
        => ex is IHasErrorCode hasCode ? hasCode.ErrorCode : CommonErrorCodes.UnknownError;

    private static ValidationProblemDetails MapValidation(PlatformValidationException ex, string? requestPath)
    {
        var errors = BuildValidationErrors(ex);
        var problem = new ValidationProblemDetails(errors)
        {
            Type = ProblemType.Validation,
            Title = "验证失败",
            Status = StatusCodes.Status400BadRequest,
            Detail = "一个或多个验证错误发生。",
            Instance = requestPath
        };
        problem.Extensions["errorCode"] = ex.ErrorCode;
        return problem;
    }

    private static Dictionary<string, string[]> BuildValidationErrors(PlatformValidationException ex)
        => ex.Errors.Count > 0
            ? ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<string, string[]> { ["_"] = [ex.Message] };

    private static ProblemDetails BuildProblemDetails(
        Exception ex,
        string? requestPath,
        ErrorDescriptor descriptor,
        bool includeExceptionDetail)
    {
        var status = ResolveHttpStatus(ex, descriptor);
        var problem = new ProblemDetails
        {
            Type = ProblemType.FromStatusCode(status),
            Title = descriptor.Title,
            Status = status,
            Detail = ResolveDetail(ex, descriptor, includeExceptionDetail),
            Instance = requestPath
        };

        problem.Extensions["errorCode"] = descriptor.ErrorCode;

        if (ex is DomainException de)
            problem.Type = ProblemType.Domain;

        return problem;
    }

    private static int ResolveHttpStatus(Exception ex, ErrorDescriptor descriptor)
    {
        if (ex is InfrastructureException { HttpStatusCode: { } overrideStatus })
            return overrideStatus;
        return descriptor.HttpStatusCode;
    }

    private static string ResolveDetail(Exception ex, ErrorDescriptor descriptor, bool includeExceptionDetail)
    {
        if (descriptor.ErrorCode == CommonErrorCodes.UnknownError && includeExceptionDetail)
            return ex.ToString();
        return ex.Message;
    }
}
