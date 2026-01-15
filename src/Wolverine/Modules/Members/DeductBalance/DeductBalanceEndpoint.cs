using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.DeductBalance;

/// <summary>
/// 扣减余额端点
/// Deduct balance endpoint
/// </summary>
public sealed class DeductBalanceEndpoint
{
    [WolverinePost("/api/members/{memberId:guid}/deduct")]
    public static async Task<IResult> Post(
        Guid memberId,
        DeductBalanceRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var command = new DeductBalance(
            memberId,
            request.Amount,
            request.Reason
        );

        var @event = await bus.InvokeAsync<BalanceDeducted>(command, cancellationToken);

        return Results.Ok(@event.MemberId);
    }

    public sealed record DeductBalanceRequest(
        decimal Amount,
        string Reason
    );
}
