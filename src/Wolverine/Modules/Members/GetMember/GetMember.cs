namespace Zss.BilliardHall.Modules.Members.GetMember;

/// <summary>
/// 查询会员信息
/// Get member query
/// </summary>
public sealed record GetMember(Guid MemberId);

/// <summary>
/// 会员 DTO
/// Member DTO
/// </summary>
public sealed record MemberDto(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    MemberTier Tier,
    decimal Balance,
    int Points
);
