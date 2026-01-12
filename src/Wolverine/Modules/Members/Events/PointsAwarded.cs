namespace Zss.BilliardHall.Modules.Members.Events;

using Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 积分已赠送事件
/// Points awarded event
/// </summary>
public sealed record PointsAwarded : IntegrationEventBase
{
    public Guid MemberId { get; init; }
    public int Points { get; init; }
    public string Reason { get; init; }
    public DateTimeOffset AwardedAt { get; init; }

    public PointsAwarded(
        Guid memberId,
        int points,
        string reason,
        DateTimeOffset awardedAt,
        Guid correlationId) : base(correlationId)
    {
        MemberId = memberId;
        Points = points;
        Reason = reason;
        AwardedAt = awardedAt;
    }
}
