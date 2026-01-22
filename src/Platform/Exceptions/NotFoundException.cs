namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// 未找到异常，用于表示请求的资源不存在
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// 创建未找到异常
    /// </summary>
    /// <param name="resourceType">资源类型</param>
    /// <param name="resourceId">资源ID</param>
    public NotFoundException(string resourceType, object resourceId)
        : base("NOT_FOUND", $"{resourceType} with id '{resourceId}' was not found.")
    {
    }

    /// <summary>
    /// 创建未找到异常，带自定义消息
    /// </summary>
    /// <param name="message">错误消息</param>
    public NotFoundException(string message)
        : base("NOT_FOUND", message)
    {
    }
}
