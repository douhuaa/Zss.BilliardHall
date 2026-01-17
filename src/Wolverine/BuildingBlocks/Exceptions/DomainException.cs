using System.Net;

namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

/// <summary>
/// 错误码（包含 Code、HTTP 状态码、消息）
/// </summary>
public record ErrorCode(string Code, HttpStatusCode StatusCode, string Message);

/// <summary>
/// 领域异常（持有 ErrorCode，统一异常响应格式）
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// 结构化错误码（包含 Code/StatusCode/Message）
    /// </summary>
    public ErrorCode ErrorCode { get; }

    public DomainException(ErrorCode errorCode)
        : base(errorCode.Message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(ErrorCode errorCode, Exception innerException)
        : base(errorCode.Message, innerException)
    {
        ErrorCode = errorCode;
    }
}
