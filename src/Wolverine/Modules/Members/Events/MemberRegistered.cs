namespace Zss.BilliardHall.Modules.Members.Events;

using Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 会员已注册事件
/// Member registered event
/// </summary>
public sealed record MemberRegistered : IntegrationEventBase
{
    public Guid MemberId { get; init; }
    public string Name { get; init; }
    public string Phone { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }

    public MemberRegistered(
        Guid memberId,
        string name,
        string phone,
        DateTimeOffset registeredAt,
        Guid correlationId) : base(correlationId)
    {
        MemberId = memberId;
        Name = name;
        Phone = phone;
        RegisteredAt = registeredAt;
    }
}
