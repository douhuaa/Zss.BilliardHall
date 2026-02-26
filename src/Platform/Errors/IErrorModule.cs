namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 定义模块错误码注册的契约接口。
/// </summary>
/// <remarks>
/// 每个业务模块应实现一个 <c>IErrorModule</c>，在应用启动阶段将本模块所有错误码注册到
/// <see cref="ErrorRegistry"/> 中。实现类命名建议遵循 <c>{ModuleName}ErrorModule</c> 约定，
/// 并在 <see cref="Register"/> 中调用 <c>ErrorRegistry.Register(...)</c>，不应包含其他业务逻辑。
/// </remarks>
public interface IErrorModule
{
    /// <summary>
    /// 将当前模块声明的所有错误码注册到 <see cref="ErrorRegistry"/> 中。
    /// 应在应用启动阶段被调用一次，完成本模块所有错误码的集中注册。
    /// </summary>
    void Register();
}

