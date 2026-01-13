namespace Zss.BilliardHall.Modules.Members.Events;

/// <summary>
/// 积分已赠送事件
/// Points awarded event
/// </summary>
public sealed record PointsAwarded(
    Guid MemberId,
    int Points,
    string? Reason = null,
    DateTimeOffset AwardedAt = default
);
