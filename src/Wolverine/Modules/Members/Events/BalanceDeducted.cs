namespace Zss.BilliardHall.Modules.Members.Events;

/// <summary>
/// 余额已扣减事件
/// Balance deducted event
/// </summary>
public sealed record BalanceDeducted(
    Guid MemberId,
    decimal Amount,
    decimal OldBalance,
    decimal NewBalance,
    string Reason,
    DateTimeOffset DeductedAt
);
