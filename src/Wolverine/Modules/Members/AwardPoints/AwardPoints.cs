namespace Zss.BilliardHall.Modules.Members.AwardPoints;

/// <summary>
/// 赠送积分命令
/// Award points command
/// </summary>
public sealed record AwardPoints(
    Guid MemberId,
    int Points,
    string Reason
);
