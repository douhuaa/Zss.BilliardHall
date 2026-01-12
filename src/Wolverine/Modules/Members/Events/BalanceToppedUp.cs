namespace Zss.BilliardHall.Modules.Members.Events;

/// <summary>
/// 余额已充值事件
/// Balance topped up event
/// </summary>
public sealed record BalanceToppedUp(
    Guid MemberId,
    decimal Amount,
    decimal OldBalance,
    decimal NewBalance,
    DateTimeOffset ToppedUpAt
);
