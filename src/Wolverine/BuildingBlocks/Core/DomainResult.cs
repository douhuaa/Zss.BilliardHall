namespace Zss.BilliardHall.BuildingBlocks.Core;

public sealed class DomainResult
{
    public bool IsSuccess { get; }
    
    /// <summary>
    /// 旧版错误码（向后兼容）
    /// Legacy error code (backward compatibility)
    /// </summary>
    [Obsolete("使用 ErrorDescriptor 代替。Use ErrorDescriptor instead.")]
    public ErrorCode? Error { get; }
    
    /// <summary>
    /// 错误描述符（推荐）
    /// Error descriptor (recommended)
    /// </summary>
    public ErrorDescriptor? ErrorDescriptor { get; }

    private DomainResult(bool isSuccess, ErrorCode? error = null, ErrorDescriptor? errorDescriptor = null)
    {
        IsSuccess = isSuccess;
#pragma warning disable CS0618 // Type or member is obsolete
        Error = error;
#pragma warning restore CS0618
        ErrorDescriptor = errorDescriptor;
    }

    public static DomainResult Success() => new(true);

    /// <summary>
    /// 创建失败结果（旧版 - 向后兼容）
    /// Create failure result (legacy - backward compatibility)
    /// </summary>
    [Obsolete("使用 Fail(ErrorDescriptor) 代替。Use Fail(ErrorDescriptor) instead.")]
    public static DomainResult Fail(ErrorCode error)
        => new(false, error: error);

    /// <summary>
    /// 创建失败结果（推荐）
    /// Create failure result (recommended)
    /// </summary>
    public static DomainResult Fail(ErrorDescriptor errorDescriptor)
        => new(false, errorDescriptor: errorDescriptor);

    public void EnsureSuccess()
    {
        if (!IsSuccess)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var errorInfo = ErrorDescriptor != null
                ? $"{ErrorDescriptor.Code}: {ErrorDescriptor.FormatMessage()}"
                : Error?.ToString() ?? "Unknown error";
#pragma warning restore CS0618

            throw new InvalidOperationException(
                $"Domain rule violated: {errorInfo}");
        }
    }
}
