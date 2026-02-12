using Marten;

namespace Zss.BilliardHall.Modules.Members.Features.GetMemberById;

/// <summary>
/// 查询会员处理器
/// 职责：执行查询，返回 DTO
/// 垂直切片原则：Query Handler 允许使用 Contracts（白名单场景）
/// </summary>
public class GetMemberByIdQueryHandler
{
    public static async Task<MemberDto?> Handle(GetMemberByIdQuery query, IDocumentSession session)
    {
        var member = await session.LoadAsync<CreateMember.Member>(query.MemberId);
        
        if (member == null)
        {
            return null;
        }

        return new MemberDto
        {
            Id = member.Id,
            Name = member.Name,
            Email = member.Email,
            PhoneNumber = member.PhoneNumber,
            CreatedAt = member.CreatedAt
        };
    }
}
