namespace Zss.BilliardHall.Modules.Orders.Features.CreateOrder;

/// <summary>
/// 创建订单命令
/// 职责：表达业务意图
/// </summary>
public record CreateOrderCommand
{
    public required Guid MemberId { get; init; }
    public required int TableNumber { get; init; }
    public DateTime? StartTime { get; init; }
}
