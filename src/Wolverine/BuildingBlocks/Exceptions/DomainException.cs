namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

/// <summary>
/// 领域异常基类
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    // ② 标准路径：用 ErrorCode 的默认 message
    public DomainException(ErrorCode error)
        : base(error.Message)
    {
        Code = error.Code;
        StatusCode = error.StatusCode;
    }

    // ✅ ③ 正确姿势：ErrorCode + 运行时上下文 message
    public DomainException(ErrorCode error, string message)
        : base(message)
    {
        Code = error.Code;
        StatusCode = error.StatusCode;
    }
}

public record ErrorCode(string Code, int StatusCode, string Message);