using Microsoft.AspNetCore.Mvc;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// Wolverine HTTP 异常处理中间件
/// 职责：在 Wolverine HTTP 处理异常时捕获并返回正确的 ProblemDetails
///
/// 这是专门针对 Wolverine HTTP 端点的异常处理，
/// 在这里捕获的异常会被转换为 HTTP 响应
/// </summary>
public sealed class WolverineHttpExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WolverineHttpExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public WolverineHttpExceptionMiddleware(
        RequestDelegate next,
        ILogger<WolverineHttpExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Wolverine HTTP 验证异常: {Errors}",
                string.Join("; ", ex.Errors.Select(kv => $"{kv.Key}: {string.Join(",", kv.Value)}")));

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var errors = ex.Errors.Count > 0
                ? ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)
                : new Dictionary<string, string[]> { ["_"] = [ex.Message] };

            var problem = new ValidationProblemDetails(errors)
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
                Title = "验证失败",
                Status = StatusCodes.Status400BadRequest,
                Detail = "一个或多个验证错误发生。",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Wolverine HTTP 业务异常: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.10",
                Title = "业务规则违反",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["errorCode"] = ex.ErrorCode;

            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (InfrastructureException ex)
        {
            var status = ex.HttpStatusCode ?? StatusCodes.Status503ServiceUnavailable;
            _logger.LogError("Wolverine HTTP 基础设施异常: {Message}", ex.Message);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
                Title = "服务暂时不可用",
                Status = status,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

