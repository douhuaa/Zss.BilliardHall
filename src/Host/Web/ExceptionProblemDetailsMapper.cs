using Microsoft.AspNetCore.Mvc;
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
    public ProblemDetails Map(Exception ex, string? requestPath, bool includeExceptionDetail) =>
        ex switch
        {
            PlatformValidationException ve => MapValidation(ve, requestPath),
            DomainException de => MapDomain(de, requestPath),
            InfrastructureException ie => MapInfrastructure(ie, requestPath),
            _ => MapUnknown(ex, requestPath, includeExceptionDetail)
        };

    private static ValidationProblemDetails MapValidation(PlatformValidationException ex, string? requestPath)
    {
        var errors = ex.Errors.Count > 0
            ? ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<string, string[]> { ["_"] = [ex.Message] };

        return new ValidationProblemDetails(errors)
        {
            Type = ProblemType.Validation,
            Title = "验证失败",
            Status = StatusCodes.Status400BadRequest,
            Detail = "一个或多个验证错误发生。",
            Instance = requestPath
        };
    }

    private static ProblemDetails MapDomain(DomainException ex, string? requestPath)
    {
        var problem = new ProblemDetails
        {
            Type = ProblemType.Domain,
            Title = "业务规则违反",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Instance = requestPath
        };
        problem.Extensions["errorCode"] = ex.ErrorCode;
        return problem;
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

    private static ProblemDetails MapUnknown(Exception ex, string? requestPath, bool includeExceptionDetail)
    {
        return new ProblemDetails
        {
            Type = ProblemType.FromStatusCode(StatusCodes.Status500InternalServerError),
            Title = "服务器内部错误",
            Status = StatusCodes.Status500InternalServerError,
            Detail = includeExceptionDetail ? ex.ToString() : "发生未处理异常，请联系管理员。",
            Instance = requestPath
        };
    }
}
