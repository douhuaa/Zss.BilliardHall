using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.BuildingBlocks.Behaviors;
using Zss.BilliardHall.BuildingBlocks.Core;

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public static async Task<IResult> Post(
        Guid memberId,
        TopUpBalanceRequest request,
        IMessageBus bus,
        ILogger<TopUpBalanceEndpoint> logger)
    {
        try
        {
            var command = new TopUpBalance(
                memberId,
                request.Amount,
                request.PaymentMethod
            );

            await bus.InvokeAsync(command);

            return Results.Ok(new { message = "充值成功" });
        }
        catch (MembersDomainException ex)
        {
            var (result, statusCode) = DomainExceptionHandler.ToResult(ex, logger);
            return Results.Json(
                new { error = result.Error, code = result.ErrorCode },
                statusCode: statusCode
            );
        }
    }

    public sealed record TopUpBalanceRequest(
        decimal Amount,
        string PaymentMethod
    );
}
