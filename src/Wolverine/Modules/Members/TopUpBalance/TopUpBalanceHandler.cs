using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Exceptions;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 充值余额命令处理器
/// Top up balance command handler
/// </summary>
public sealed class TopUpBalanceHandler
{
    [Transactional]
    public async Task<(Guid memberId, BalanceToppedUp Event)> Handle(
        TopUpBalance command,
        IDocumentSession session,
        ILogger<TopUpBalanceHandler> logger,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            throw new DomainException(MemberErrorCodes.NotFound);

        member.TopUp(command.Amount);

        session.Store(member);

        var @event = new BalanceToppedUp(
            member.Id,
            command.Amount,
            DateTimeOffset.UtcNow
        );

        logger.LogInformation(
            "会员充值成功: {MemberId}, 金额: {Amount:F2}, 余额: {Balance:F2}",
            member.Id,
            command.Amount,
            member.Balance
        );

        return (member.Id, @event);
    }
}
