namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 集成事件基类，提供统一的事件标识和关联 ID
/// Base class for integration events, provides unified event ID and correlation ID
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    /// <summary>
    /// 事件 ID（唯一标识此事件实例）
    /// Event ID (uniquely identifies this event instance)
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 关联 ID（用于跟踪跨服务调用链，便于 Saga/对账/审计）
    /// Correlation ID (used for tracking across service calls, helpful for Saga/reconciliation/audit)
    /// </summary>
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// 事件发生时间（UTC）
    /// Event occurred time (UTC)
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// 构造函数：自动设置事件 ID 和发生时间
    /// Constructor: automatically sets event ID and occurred time
    /// </summary>
    /// <param name="correlationId">关联 ID，用于跟踪业务流程</param>
    /// <exception cref="ArgumentException">当 correlationId 为空时抛出</exception>
    protected IntegrationEventBase(Guid correlationId)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId 不能为空", nameof(correlationId));

        CorrelationId = correlationId;
        OccurredAt = DateTimeOffset.UtcNow;
    }
}
