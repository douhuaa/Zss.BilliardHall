using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.BuildingBlocks.Contracts;

namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员端点
/// Register member endpoint
/// </summary>
public sealed class RegisterMemberEndpoint
{
    [WolverinePost("/api/members/register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Post(
        RegisterMemberRequest request,
        IMessageBus bus)
    {
        var command = new RegisterMember(
            request.Name,
            request.Phone,
            request.Email,
            request.Password
        );

        var result = await bus.InvokeAsync<Result<Guid>>(command);

        return result.IsSuccess
            ? Results.Ok(new { memberId = result.Value })
            : Results.BadRequest(new { error = result.Error });
    }

    public sealed record RegisterMemberRequest(
        string Name,
        string Phone,
        string Email,
        string Password
    );
}
