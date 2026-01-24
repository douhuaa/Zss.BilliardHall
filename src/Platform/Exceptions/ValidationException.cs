namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// ADR-240: 验证异常，表示输入数据不符合契约或约束
/// Exception thrown when input data does not meet contract or constraint requirements
/// </summary>
/// <remarks>
/// 验证异常特征：
/// - 不可重试（输入错误不会因重试而修正）
/// - 必须在 Handler 入口处抛出
/// - 应包含具体的验证失败详情
/// </remarks>
public class ValidationException : Exception
{
    /// <summary>
    /// 验证失败的字段和错误消息集合
    /// Collection of field names and their validation error messages
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
