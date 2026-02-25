using FluentValidation;
using Microsoft.Extensions.Logging;
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
public class FluentValidationExceptionTranslator : IExceptionTranslator
{
    private readonly ILogger<FluentValidationExceptionTranslator> _logger;

    public FluentValidationExceptionTranslator(ILogger<FluentValidationExceptionTranslator> logger)
    {
        _logger = logger;
    }

    public Exception? Translate(Exception ex)
    {
        if (ex is not ValidationException validationEx)
        {
            _logger.LogDebug("异常不是 ValidationException，跳过转换。异常类型: {ExceptionType}", ex.GetType().Name);
            return null;
        }

        _logger.LogDebug("开始转换 FluentValidation.ValidationException，共 {ErrorCount} 个验证错误", validationEx.Errors.Count());

        // 将 FluentValidation 的验证错误转换为字典格式
        var errorDictionary = validationEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        _logger.LogDebug("转换后的错误字段数: {FieldCount}", errorDictionary.Count);
        foreach (var kvp in errorDictionary)
        {
            _logger.LogDebug("字段 '{Field}' 有 {ErrorCount} 个错误: {Errors}",
                kvp.Key, kvp.Value.Length, string.Join("; ", kvp.Value));
        }

        // 转换为平台层的验证异常
        var platformEx = new PlatformValidationException(
            "验证失败，请检查输入数据。",
            errorDictionary);

        _logger.LogDebug("成功转换为 Platform.Exceptions.ValidationException");
        return platformEx;
    }
}

