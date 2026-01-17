using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 充值余额端点
/// Top up balance endpoint
/// </summary>
public sealed class TopUpBalanceEndpoint
{
    [WolverinePost("/api/members/{memberId:guid}/topup")]
    public static async Task<IResult> Post(
        Guid memberId,
        TopUpBalanceRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var command = new TopUpBalance(
            memberId,
            request.Amount,
            request.PaymentMethod
        );

        var result = await bus.InvokeAsync<(Guid memberId, BalanceToppedUp Event)>(command, cancellationToken);
        
        return Results.Ok(result.memberId);
    }

    public sealed record TopUpBalanceRequest(
        decimal Amount,
        string PaymentMethod
    );
}
