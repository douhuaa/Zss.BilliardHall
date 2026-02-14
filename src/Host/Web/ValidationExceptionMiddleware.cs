using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 处理 FluentValidation 验证异常，转换为标准 ProblemDetails 响应
/// 确保 Web API 返回一致的错误格式
/// </summary>
public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "验证失败：{Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "验证失败",
            Status = StatusCodes.Status400BadRequest,
            Detail = "一个或多个验证错误发生。",
            Instance = context.Request.Path
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(problemDetails, options);
    }
}
