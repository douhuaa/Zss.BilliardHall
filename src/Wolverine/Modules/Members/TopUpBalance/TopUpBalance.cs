namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 充值余额命令
/// Top up balance command
/// </summary>
public sealed record TopUpBalance(
    Guid MemberId,
    decimal Amount,
    string PaymentMethod
);
