namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// 领域异常基类，用于表示业务规则违反或领域逻辑错误
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// 创建领域异常
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="message">错误消息</param>
    public DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 创建领域异常，带内部异常
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
