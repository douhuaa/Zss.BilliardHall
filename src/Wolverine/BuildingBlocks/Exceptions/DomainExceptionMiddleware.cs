using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

public sealed class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(
        RequestDelegate next,
        ILogger<DomainExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "DomainException {Code}, TraceId={TraceId}, Metadata={Metadata}",
                ex.Code,
                traceId,
                ex.Metadata
            );

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(
                new
                {
                    code = ex.Code,
                    message = ex.Code switch
                    {
                        _ => "请求不合法",
                    },
                    traceId,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception, TraceId={TraceId}", traceId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(
                new
                {
                    code = "System.Unhandled",
                    message = "系统错误",
                    traceId,
                }
            );
        }
    }
}
