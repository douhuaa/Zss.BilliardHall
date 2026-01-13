using Microsoft.AspNetCore.Http;

namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

/// <summary>
/// 领域异常基类
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string? message = null)
        : base(message)
    {
        Code = code;
    }

    public DomainException(ErrorCode code)
        : base(code.Message)
    {
        Code = code.Code;
    }

    public DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

public record ErrorCode(string Code, string Message);

public sealed record ErrorResponse(string Code, string Message);

public static class DomainExceptionHttpMapper
{
    public static int MapStatusCode(DomainException ex) => ex.Code switch
    {
        "Payment.Failed" => StatusCodes.Status402PaymentRequired,
        // 资源状态不满足（余额不足/库存不足等）→ 409
        _ when ex.Code.Contains("Insufficient", StringComparison.OrdinalIgnoreCase) =>
            StatusCodes.Status409Conflict,

        // 重复（手机号已存在、记录已存在）→ 409（与现有状态冲突）
        _ when ex.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) =>
            StatusCodes.Status409Conflict,

        // 明确的“禁止访问/操作”→ 403
        _ when ex.Code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) =>
            StatusCodes.Status403Forbidden,

        // 典型资源未找到 → 404
        _ when ex.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase) =>
            StatusCodes.Status404NotFound,

        // 显式 Conflict 关键字 → 409
        _ when ex.Code.Contains("Conflict", StringComparison.OrdinalIgnoreCase) =>
            StatusCodes.Status409Conflict,

        // 其余都按 BadRequest 处理
        _ => StatusCodes.Status400BadRequest,
    };
}

public sealed class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public DomainExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = DomainExceptionHttpMapper.MapStatusCode(ex);

            context.Response.ContentType = "application/json";

            var response = new ErrorResponse(ex.Code, ex.Message ?? ex.Code);

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
