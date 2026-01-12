namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 表示操作结果，包含成功/失败状态和错误信息
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    /// <summary>
    /// 结构化错误代码，格式：Area:Key（如 Billing:TableUnavailable）
    /// 用于前端错误识别、日志聚合和重试策略
    /// </summary>
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string error, string? errorCode = null)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("成功的结果不能包含错误信息");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("失败的结果必须包含错误信息");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Fail(string error) => new(false, error);
    public static Result Fail(string error, string errorCode) => new(false, error, errorCode);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty, null);
    // 使用 default! 是安全的：对于失败结果，访问 Value 会先抛出异常，因此底层 _value 永远不会在失败状态下被读取
    public static Result<T> Fail<T>(string error) => new(default!, false, error, null);
    public static Result<T> Fail<T>(string error, string errorCode) => new(default!, false, error, errorCode);
}

/// <summary>
/// 表示带返回值的操作结果
/// </summary>
public class Result<T> : Result
{
    private readonly T _value;

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("失败的结果不能访问 Value");
            return _value;
        }
    }

    internal Result(T value, bool isSuccess, string error, string? errorCode)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }
}
