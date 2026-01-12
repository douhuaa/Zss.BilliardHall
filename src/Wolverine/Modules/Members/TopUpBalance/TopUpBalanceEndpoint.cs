using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.BuildingBlocks.Contracts;

namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 充值余额端点
/// Top up balance endpoint
/// </summary>
public sealed class TopUpBalanceEndpoint
{
    [WolverinePost("/api/members/{memberId:guid}/topup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Post(
        Guid memberId,
        TopUpBalanceRequest request,
        IMessageBus bus)
    {
        var command = new TopUpBalance(
            memberId,
            request.Amount,
            request.PaymentMethod
        );

        var result = await bus.InvokeAsync<Result>(command);

        return result.IsSuccess
            ? Results.Ok(new { message = "充值成功" })
            : Results.BadRequest(new { error = result.Error });
    }

    public sealed record TopUpBalanceRequest(
        decimal Amount,
        string PaymentMethod
    );
}
