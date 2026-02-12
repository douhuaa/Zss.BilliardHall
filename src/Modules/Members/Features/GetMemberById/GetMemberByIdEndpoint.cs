using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;

namespace Zss.BilliardHall.Modules.Members.Features.GetMemberById;

/// <summary>
/// 查询会员端点
/// 职责：HTTP 请求/响应处理
/// </summary>
public static class GetMemberByIdEndpoint
{
    [WolverineGet("/api/members/{memberId}")]
    public static async Task<IResult> Get(Guid memberId, IMessageBus bus)
    {
        var query = new GetMemberByIdQuery { MemberId = memberId };
        var member = await bus.InvokeAsync<MemberDto?>(query);
        
        return member == null 
            ? Results.NotFound() 
            : Results.Ok(member);
    }
}
