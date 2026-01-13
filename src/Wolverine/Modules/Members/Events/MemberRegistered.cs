namespace Zss.BilliardHall.Modules.Members.Events;

/// <summary>
/// 会员已注册事件
/// Member registered event
/// </summary>
public sealed record MemberRegistered(
    Guid MemberId,
    string Name,
    string Phone,
    DateTimeOffset RegisteredAt = default
);
