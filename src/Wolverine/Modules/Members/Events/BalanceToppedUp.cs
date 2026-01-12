namespace Zss.BilliardHall.Modules.Members.Events;

using Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 余额已充值事件
/// Balance topped up event
/// </summary>
public sealed record BalanceToppedUp : IntegrationEventBase
{
    public Guid MemberId { get; init; }
    public decimal Amount { get; init; }
    public decimal OldBalance { get; init; }
    public decimal NewBalance { get; init; }
    public DateTimeOffset ToppedUpAt { get; init; }

    public BalanceToppedUp(
        Guid memberId,
        decimal amount,
        decimal oldBalance,
        decimal newBalance,
        DateTimeOffset toppedUpAt,
        Guid correlationId) : base(correlationId)
    {
        MemberId = memberId;
        Amount = amount;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        ToppedUpAt = toppedUpAt;
    }
}
