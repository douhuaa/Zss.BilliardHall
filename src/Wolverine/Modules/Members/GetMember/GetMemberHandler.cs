using Marten;
using Zss.BilliardHall.BuildingBlocks.Contracts;

namespace Zss.BilliardHall.Modules.Members.GetMember;

/// <summary>
/// 查询会员信息处理器
/// Get member query handler
/// </summary>
public sealed class GetMemberHandler
{
    public async Task<Result<MemberDto>> Handle(
        GetMember query,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(query.MemberId, ct);

        if (member == null)
            return Result.Fail<MemberDto>("会员不存在");

        var dto = new MemberDto(
            member.Id,
            member.Name,
            member.Phone,
            member.Email,
            member.Tier,
            member.Balance,
            member.Points,
            member.RegisteredAt
        );

        return Result.Success(dto);
    }
}
