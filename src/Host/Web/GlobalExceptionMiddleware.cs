using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 全局异常处理中间件，统一将所有未处理异常转换为 application/problem+json 响应。
/// 覆盖 Domain / Infrastructure / Unknown 异常，取代已移除的 ValidationExceptionMiddleware。
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private const string ProblemJsonContentType = "application/problem+json";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IExceptionProblemDetailsMapper _mapper;
    private readonly IEnumerable<IExceptionTranslator> _translators;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment,
        IExceptionProblemDetailsMapper mapper,
        IEnumerable<IExceptionTranslator> translators)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _mapper = mapper;
        _translators = translators;
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
        var translated = TranslateException(ex);
        var includeDetail = _environment.IsDevelopment();
        var problem = _mapper.Map(translated, context.Request.Path, includeDetail);
        problem.AddTraceInfo(context);

        var status = problem.Status ?? StatusCodes.Status500InternalServerError;
        LogException(ex, translated, status);

        context.Response.StatusCode = status;
        context.Response.ContentType = ProblemJsonContentType;
        // 必须使用运行时类型序列化，否则 ValidationProblemDetails.Errors 字段会丢失
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: ProblemJsonContentType);
    }

    private void LogException(Exception original, Exception effective, int status)
    {
        if (status >= 500)
        {
            _logger.LogError(original, "服务器错误：{ExceptionType} - {Message}", effective.GetType().Name, effective.Message);
            return;
        }

        _logger.LogWarning(original, "客户端错误：{ExceptionType} - {Message}", effective.GetType().Name, effective.Message);
    }

    private Exception TranslateException(Exception ex)
    {
        foreach (var translator in _translators)
        {
            var translated = translator.Translate(ex);
            if (translated is not null) return translated;
        }

        return ex;
    }
}

