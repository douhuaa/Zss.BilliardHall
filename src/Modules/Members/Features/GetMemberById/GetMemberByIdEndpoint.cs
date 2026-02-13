namespace Zss.BilliardHall.Modules.Members.Features.GetMemberById;

/// <summary>
/// 查询会员端点
/// 职责：HTTP 请求/响应处理
/// </summary>
public static class GetMemberByIdEndpoint
{
    [WolverineGet("/api/members/{memberId}")]
    public static async Task<IResult> Get(Guid memberId, IMessageBus bus, CancellationToken ct = default)
    {
        var query = new GetMemberByIdQuery(memberId);
        var member = await bus.InvokeAsync<MemberDto?>(query, ct);

        return member == null ? Results.NotFound() : Results.Ok(member);
    }
}
