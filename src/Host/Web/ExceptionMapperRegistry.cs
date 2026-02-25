using Wolverine.Http;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// Wolverine HTTP 异常映射规则
/// 职责：将域异常映射为 HTTP 响应
///
/// Wolverine 会自动使用这些规则来处理 Handler 抛出的异常
/// 异常 → Wolverine 捕获 → 查找对应的映射规则 → 返回 HTTP 响应
/// </summary>
public class ExceptionMapperRegistry
{
    /// <summary>
    /// 映射验证异常
    /// </summary>
    public static void MapValidationException(ValidationException ex, HttpContext context, ILogger logger)
    {
        logger.LogWarning("验证异常: {Errors}",
            string.Join("; ", ex.Errors.Select(kv => $"{kv.Key}: {string.Join(",", kv.Value)}")));

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var errors = ex.Errors.Count > 0
            ? ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<string, string[]> { ["_"] = [ex.Message] };

        var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(errors)
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            Title = "验证失败",
            Status = StatusCodes.Status400BadRequest,
            Detail = "一个或多个验证错误发生。",
            Instance = context.Request.Path
        };

        context.Response.WriteAsJsonAsync(problemDetails);
    }

    /// <summary>
    /// 映射业务规则异常
    /// </summary>
    public static void MapDomainException(DomainException ex, HttpContext context, ILogger logger)
    {
        logger.LogWarning("业务规则违反: {Message}", ex.Message);

        context.Response.StatusCode = StatusCodes.Status409Conflict;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.10",
            Title = "业务规则违反",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Instance = context.Request.Path
        };
        problemDetails.Extensions["errorCode"] = ex.ErrorCode;

        context.Response.WriteAsJsonAsync(problemDetails);
    }

    /// <summary>
    /// 映射基础设施异常
    /// </summary>
    public static void MapInfrastructureException(InfrastructureException ex, HttpContext context, ILogger logger)
    {
        logger.LogError("基础设施异常: {Message}", ex.Message);

        var status = ex.HttpStatusCode ?? StatusCodes.Status503ServiceUnavailable;
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
            Title = "服务暂时不可用",
            Status = status,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        context.Response.WriteAsJsonAsync(problemDetails);
    }
}

