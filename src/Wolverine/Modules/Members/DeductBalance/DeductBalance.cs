namespace Zss.BilliardHall.Modules.Members.DeductBalance;

/// <summary>
/// 扣减余额命令（内部命令，不暴露 HTTP 端点）
/// Deduct balance command (internal command, no HTTP endpoint)
/// </summary>
public sealed record DeductBalance(
    Guid MemberId,
    decimal Amount,
    string Reason
);
