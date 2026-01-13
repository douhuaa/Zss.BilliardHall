using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员端点
/// Register member endpoint
/// </summary>
public sealed class RegisterMemberEndpoint
{
    [WolverinePost("/api/members/register")]
    public static async Task<IResult> Post(RegisterMemberRequest request, IMessageBus bus)
    {
        var command = new RegisterMember(
            request.Name,
            request.Phone,
            request.Email,
            request.Password
        );

        var (memberId, _) = await bus.InvokeAsync<(Guid, MemberRegistered)>(command);

        return Results.Ok(new { memberId });
    }

    public sealed record RegisterMemberRequest(
        string Name,
        string Phone,
        string Email,
        string Password
    );
}
