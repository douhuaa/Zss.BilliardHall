namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 错误码注册中心契约（DI 单例）
/// </summary>
public interface IErrorRegistry
{
    /// <summary>查找错误码描述符，不存在时返回 null</summary>
    ErrorDescriptor? Find(string errorCode);

    /// <summary>查找错误码描述符，不存在时 fallback 到 COMMON_UNKNOWN_ERROR</summary>
    ErrorDescriptor GetOrFallback(string errorCode);

    /// <summary>注册一个错误码描述符（启动时由 IErrorRegistrar 调用）</summary>
    void Register(ErrorDescriptor descriptor);
}
