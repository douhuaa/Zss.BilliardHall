namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// ADR-240: 领域异常基类，表示业务规则违反
/// Base class for domain exceptions that represent business rule violations
/// </summary>
/// <remarks>
/// 领域异常特征：
/// - 不可重试（业务规则违反不会因重试而改变）
/// - 必须在领域模型中抛出
/// - 应包含业务语义的错误代码和消息
/// </remarks>
public abstract class DomainException : Exception
{
    /// <summary>
    /// 业务错误代码
    /// Business error code for client handling
    /// </summary>
    public string ErrorCode { get; }

    protected DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
