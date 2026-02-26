using Microsoft.AspNetCore.Mvc;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 全局异常处理中间件，统一将所有未处理异常转换为 application/problem+json 响应。
/// 覆盖 Domain / Infrastructure / Unknown 异常，取代已移除的 ValidationExceptionMiddleware。
/// 
/// 职责：
/// - 记录日志
/// - 调用 mapper 映射异常为 ProblemDetails
/// - 输出 application/problem+json 响应
/// 
/// 注意：异常转换已前移到 Wolverine pipeline 层（FluentValidation、PostgresException 等），
/// 此中间件不再进行异常翻译，只做映射输出。
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private const string ProblemJsonContentType = "application/problem+json";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IExceptionProblemDetailsMapper _mapper;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment,
        IExceptionProblemDetailsMapper mapper)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _mapper = mapper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "响应已开始，无法修改响应。异常类型：{ExceptionType}", ex.GetType().Name);
            throw;
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var problem = BuildProblemDetails(context, ex);
        var status = problem.Status ?? StatusCodes.Status500InternalServerError;

        LogException(ex, status);

        await WriteProblemDetailsAsync(context, problem, status);
    }

    private ProblemDetails BuildProblemDetails(HttpContext context, Exception ex)
    {
        var includeDetail = _environment.IsDevelopment();

        var problem = _mapper.Map(ex, context.Request.Path, includeDetail);
        problem.AddTraceInfo(context);

        return problem;
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problem, int status)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = ProblemJsonContentType;

        // 必须使用运行时类型序列化，否则 ValidationProblemDetails.Errors 字段会丢失
        return context.Response.WriteAsJsonAsync(
            problem,
            problem.GetType(),
            options: null,
            contentType: ProblemJsonContentType);
    }

    private void LogException(Exception ex, int status)
    {
        if (status >= 500)
        {
            _logger.LogError(ex, "服务器错误：{ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
            return;
        }

        _logger.LogWarning(ex, "客户端错误：{ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
    }
}

