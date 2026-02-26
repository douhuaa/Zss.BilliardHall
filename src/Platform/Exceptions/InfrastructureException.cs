using Zss.BilliardHall.Platform.Errors;

namespace Zss.BilliardHall.Platform.Exceptions;

/// <summary>
/// ADR-240: 基础设施异常，表示技术依赖失败
/// Exception thrown when technical dependencies (database, network, external services) fail
/// </summary>
/// <remarks>
/// 基础设施异常特征：
/// - 可能可重试（通过实现 IRetryable 标记）
/// - 必须在基础设施层抛出
/// - 应封装底层技术异常
/// </remarks>
public class InfrastructureException : Exception, IRetryable, IHasErrorCode
{
    /// <summary>
    /// 基础设施组件类型（如 Database, Network, ExternalService）
    /// Infrastructure component type (e.g., Database, Network, ExternalService)
    /// </summary>
    public string ComponentType { get; }

    /// <summary>
    /// 是否可重试
    /// Whether this exception is retryable
    /// </summary>
    public bool IsRetryable { get; }

    /// <summary>
    /// 建议的重试延迟（毫秒）
    /// Suggested retry delay in milliseconds
    /// </summary>
    public int? SuggestedRetryDelayMs { get; }

    /// <summary>
    /// HTTP 状态码覆盖（可选）。若指定，则映射时使用该状态码；否则默认 503。
    /// Optional HTTP status code override. If specified, this code is used during mapping; otherwise defaults to 503.
    /// </summary>
    /// <remarks>
    /// ⚠️ 仅用于表达真实的下游技术语义（如网关返回 502、依赖超时返回 504）。
    /// 禁止用于映射业务错误（400/409 等属于 DomainException/ValidationException 的职责）。
    /// Only use to express real downstream infrastructure semantics (e.g. 502 Bad Gateway, 504 Gateway Timeout).
    /// Do NOT use to map business errors — those belong to DomainException or ValidationException.
    /// </remarks>
    public int? HttpStatusCode { get; init; }

    /// <summary>
    /// 稳定的错误码（可选）。若未指定，默认使用 COMMON_UNKNOWN_ERROR。
    /// Stable error code. Defaults to COMMON_UNKNOWN_ERROR if not specified.
    /// </summary>
    public string ErrorCode { get; init; } = CommonErrorCodes.UnknownError;

    public InfrastructureException(
        string componentType,
        string message,
        bool isRetryable = false,
        int? suggestedRetryDelayMs = null)
        : base(message)
    {
        ComponentType = componentType;
        IsRetryable = isRetryable;
        SuggestedRetryDelayMs = suggestedRetryDelayMs;
    }

    public InfrastructureException(
        string componentType,
        string message,
        Exception innerException,
        bool isRetryable = false,
        int? suggestedRetryDelayMs = null)
        : base(message, innerException)
    {
        ComponentType = componentType;
        IsRetryable = isRetryable;
        SuggestedRetryDelayMs = suggestedRetryDelayMs;
    }
}
