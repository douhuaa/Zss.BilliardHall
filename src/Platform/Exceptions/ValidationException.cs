namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// 验证异常，用于表示输入验证失败
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// 验证错误详情
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// 创建验证异常
    /// </summary>
    /// <param name="errors">验证错误字典</param>
    public ValidationException(Dictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "一个或多个验证错误发生")
    {
        Errors = errors;
    }

    /// <summary>
    /// 创建验证异常，带单个错误消息
    /// </summary>
    /// <param name="message">错误消息</param>
    public ValidationException(string message)
        : base("VALIDATION_ERROR", message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
