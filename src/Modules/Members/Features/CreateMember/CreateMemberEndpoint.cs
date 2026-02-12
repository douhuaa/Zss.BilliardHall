using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;

namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员端点
/// 职责：HTTP 请求/响应处理
/// </summary>
public static class CreateMemberEndpoint
{
    [WolverinePost("/api/members")]
    public static async Task<IResult> Create(CreateMemberCommand command, IMessageBus bus)
    {
        var memberId = await bus.InvokeAsync<Guid>(command);
        return Results.Created($"/api/members/{memberId}", new CreateMemberResponse { Id = memberId });
    }
}
