using Marten;
using Wolverine.Http;

namespace Zss.BilliardHall.Modules.Members.DeductBalance;

/// <summary>
/// 扣减余额 HTTP 端点
/// Deduct balance HTTP endpoint
/// </summary>
public static class DeductBalanceEndpoint
{
    [WolverinePost("/api/members/{memberId:guid}/deduct-balance")]
    public static async Task<IResult> Post(
        Guid memberId,
        DeductBalance command,
        IDocumentSession session,
        CancellationToken ct)
    {
        var member = await session.LoadAsync<Member>(memberId, ct);

        if (member == null)
            throw MembersDomainErrors.MemberNotFound();

        member.Deduct(command.Amount);

        session.Update(member);
        await session.SaveChangesAsync(ct);

        return Results.Ok();
    }
}
