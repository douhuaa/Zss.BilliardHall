using FluentValidation;
using Zss.BilliardHall.Platform.Contracts;
using PlatformValidationException = Zss.BilliardHall.Platform.Exceptions.ValidationException;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// FluentValidation 异常转换器
/// 职责：将 FluentValidation.ValidationException 转换为 Platform.Exceptions.ValidationException
///
/// 工作流程：
/// 1. Wolverine + FluentValidation 自动验证命令
/// 2. 验证失败时抛出 FluentValidation.ValidationException
/// 3. GlobalExceptionMiddleware 捕获异常
/// 4. IExceptionTranslator 链将异常转换为领域异常
/// 5. ExceptionProblemDetailsMapper 将异常映射为 ProblemDetails 响应
/// </summary>
public sealed class FluentValidationExceptionTranslator(
    ILogger<FluentValidationExceptionTranslator> logger) : IExceptionTranslator
{
    public Exception? Translate(Exception ex)
    {
        if (ex is not ValidationException validationEx) return null;

        var errors = validationEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        logger.LogDebug(
        "FluentValidation 验证失败：{ErrorCount} 条错误，{FieldCount} 个字段",
        validationEx.Errors.Count(),
        errors.Count);

        return new PlatformValidationException("验证失败，请检查输入数据。", errors);
    }
}

