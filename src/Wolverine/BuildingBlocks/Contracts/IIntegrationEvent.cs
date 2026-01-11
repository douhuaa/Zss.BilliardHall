namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 集成事件标记接口，用于跨模块通信
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// 事件 ID
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// 事件发生时间（UTC）
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
