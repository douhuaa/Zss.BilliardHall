using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;

namespace Zss.BilliardHall.Modules.Members.GetMember;

/// <summary>
/// 查询会员信息端点
/// Get member endpoint
/// </summary>
public sealed class GetMemberEndpoint
{
    [WolverineGet("/api/members/{memberId:guid}")]
    public static async Task<IResult> Get(
        Guid memberId,
        IMessageBus bus)
    {
        var result = await bus.InvokeAsync<MemberDto>(
            new GetMember(memberId)
        );

        return Results.Ok(result);
    }
}
