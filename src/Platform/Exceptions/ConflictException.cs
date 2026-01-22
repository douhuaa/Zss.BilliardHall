namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// 冲突异常，用于表示操作与当前状态冲突
/// </summary>
public class ConflictException : DomainException
{
    /// <summary>
    /// 创建冲突异常
    /// </summary>
    /// <param name="message">错误消息</param>
    public ConflictException(string message)
        : base("CONFLICT", message)
    {
    }

    /// <summary>
    /// 创建冲突异常，带错误码
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="message">错误消息</param>
    public ConflictException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
