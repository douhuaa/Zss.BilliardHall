namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 全局异常处理中间件，统一将所有未处理异常转换为 application/problem+json 响应。
/// 覆盖 Domain / Infrastructure / Unknown 异常，取代已移除的 ValidationExceptionMiddleware。
/// 异常翻译（技术异常 → 领域异常）由 Wolverine pipeline 在上游负责。
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
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "响应已开始，无法修改响应。异常类型：{ExceptionType}", ex.GetType().Name);
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var includeDetail = _environment.IsDevelopment();
        var problem = _mapper.Map(ex, context.Request.Path, includeDetail);
        problem.AddTraceInfo(context);

        var status = problem.Status ?? StatusCodes.Status500InternalServerError;
        LogException(ex, status);

        context.Response.StatusCode = status;
        context.Response.ContentType = ProblemJsonContentType;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: ProblemJsonContentType);
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

