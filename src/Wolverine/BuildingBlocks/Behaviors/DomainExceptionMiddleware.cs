using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.BuildingBlocks.Behaviors;

/// <summary>
/// 领域异常中间件（统一处理 DomainException，自动映射 HTTP 状态码和错误响应）
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> _logger)
    {
        _next = next;
        this._logger = _logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await HandleDomainExceptionAsync(context, ex);
        }
    }

    private async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogWarning(
            exception,
            "Domain exception occurred. Code: {ErrorCode}, TraceId: {TraceId}",
            exception.ErrorCode.Code,
            traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)exception.ErrorCode.StatusCode;

        var response = new
        {
            code = exception.ErrorCode.Code,
            message = exception.ErrorCode.Message,
            traceId = traceId,
            timestamp = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
