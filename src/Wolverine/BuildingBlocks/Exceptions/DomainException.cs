using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

/// <summary>
/// 领域异常基类
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public object? Metadata { get; }

    // 标准路径：用 ErrorCode 的默认 message
    public DomainException(ErrorCode error)
        : base(error.Message)
    {
        Code = error.Code;
        StatusCode = error.StatusCode;
    }

    // ✅ 正确姿势：message 永远来自 ErrorCode
    // 运行时上下文进 Metadata
    // Middleware 永不序列化 Metadata
    public DomainException(ErrorCode error, object? metadata = null)
        : base(error.Message)
    {
        Code = error.Code;
        StatusCode = error.StatusCode;
        Metadata = metadata;
    }
}

// ErrorCode.Code 采用三段式：{Module}.{ Category}.{ Key}
public record ErrorCode(string Code, int StatusCode, string Message);