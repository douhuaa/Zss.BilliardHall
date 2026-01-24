namespace Zss.BilliardHall.Modules.Members.Features.GetMemberById;

/// <summary>
/// 查询会员命令
/// 职责：表达查询意图
/// </summary>
public record GetMemberByIdQuery
{
    public required Guid MemberId { get; init; }
}
