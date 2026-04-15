namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 表示领域层中携带错误码的可预期业务异常，对应 ADR-240 Handler 模式中需要通过异常机制向上传播的业务错误场景。
/// </summary>
/// <remarks>
/// 与 <c>DomainException</c> 的区别：<c>DomainException</c> 是已有的抽象基类，适用于需要自定义子类的场景；
/// <c>DomainError</c> 是轻量级的具体类，可直接实例化用于快速抛出带错误码的领域异常，无需定义额外的子类。
/// 使用边界：轻量、一次性的业务拒绝可直接抛出 <c>DomainError</c>；
/// 需要附加上下文信息或需要清晰领域语义类型时，优先继续使用 <c>DomainException</c> 子类。
/// </remarks>
public class DomainError(string code) : Exception(code)
{
    /// <summary>
    /// 领域错误的稳定错误码，用于标识错误类型（例如 <c>"ORDERS_ALREADY_PAID"</c>）。
    /// 应与 <see cref="ErrorRegistry"/> 中已注册的错误码一致。
    /// </summary>
    public string Code { get; } = code;
}

