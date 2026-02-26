namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 错误码注册中心实现（DI 单例）
/// 线程安全：注册仅发生在启动阶段，查询阶段只读
/// </summary>
public sealed class ErrorRegistry : IErrorRegistry
{
    private readonly Dictionary<string, ErrorDescriptor> _descriptors = new(StringComparer.OrdinalIgnoreCase);

    public void Register(ErrorDescriptor descriptor)
        => _descriptors[descriptor.ErrorCode] = descriptor;

    public ErrorDescriptor? Find(string errorCode)
        => _descriptors.TryGetValue(errorCode, out var d) ? d : null;

    public ErrorDescriptor GetOrFallback(string errorCode)
        => Find(errorCode) ?? Find(CommonErrorCodes.UnknownError)!;
}
