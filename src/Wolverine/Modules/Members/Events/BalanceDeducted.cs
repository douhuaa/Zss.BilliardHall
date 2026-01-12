namespace Zss.BilliardHall.Modules.Members.Events;

using Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 余额已扣减事件
/// Balance deducted event
/// </summary>
public sealed record BalanceDeducted : IntegrationEventBase
{
    public Guid MemberId { get; init; }
    public decimal Amount { get; init; }
    public decimal OldBalance { get; init; }
    public decimal NewBalance { get; init; }
    public string Reason { get; init; }
    public DateTimeOffset DeductedAt { get; init; }

    public BalanceDeducted(
        Guid memberId,
        decimal amount,
        decimal oldBalance,
        decimal newBalance,
        string reason,
        DateTimeOffset deductedAt,
        Guid correlationId) : base(correlationId)
    {
        MemberId = memberId;
        Amount = amount;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        Reason = reason;
        DeductedAt = deductedAt;
    }
}
