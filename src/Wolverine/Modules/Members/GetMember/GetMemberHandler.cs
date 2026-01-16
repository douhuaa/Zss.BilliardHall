using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace Zss.BilliardHall.Modules.Members.GetMember;

/// <summary>
/// 查询会员信息处理器
/// Get member query handler
/// </summary>
public sealed class GetMemberHandler
{
    [Transactional]
    public async Task<MemberDto> Handle(
        GetMember query,
        IDocumentSession session,
        ILogger<GetMemberHandler> logger,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(query.MemberId, ct);

        if (member == null)
            throw MembersDomainErrors.NotFound();

        var dto = new MemberDto(
            member.Id,
            member.Name,
            member.Phone,
            member.Email,
            member.Tier,
            member.Balance,
            member.Points
        );
        logger.LogInformation("查询会员信息: ID:{MemberId}, Name:{MemberName}", member.Id, member.Name);
        return dto;
    }
}
