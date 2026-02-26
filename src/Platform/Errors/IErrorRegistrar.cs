namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 模块化错误码注册接口。
/// 各模块实现此接口并通过 DI 注入，应用启动时统一调用 Register。
/// </summary>
public interface IErrorRegistrar
{
    /// <summary>向注册中心注册本模块的所有错误码描述符</summary>
    void Register(IErrorRegistry registry);
}
