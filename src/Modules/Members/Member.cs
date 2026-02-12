namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 会员实体
/// </summary>
public sealed record Member(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    DateTimeOffset CreatedAt);
