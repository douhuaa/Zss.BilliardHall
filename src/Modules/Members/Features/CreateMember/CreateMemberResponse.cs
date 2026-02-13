namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员响应
/// </summary>
public sealed record CreateMemberResponse
{
    public required Guid Id { get; init; }
}
