using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.DeductBalance;

/// <summary>
/// 扣减余额命令处理器
/// Deduct balance command handler
/// </summary>
public sealed class DeductBalanceHandler
{
    [Transactional]
    public async Task<BalanceDeducted> HandleWithCascading(
        DeductBalance command,
        IDocumentSession session,
        ILogger<DeductBalanceHandler> logger,
        CancellationToken ct = default
    )
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            throw MembersDomainErrors.NotFound(command.MemberId);

        member.Deduct(command.Amount);

        session.Store(member);

        // 返回级联消息（Wolverine 会自动发布）
        var @event = new BalanceDeducted(
            member.Id,
            command.Amount,
            command.Reason,
            DateTimeOffset.UtcNow
        );

        logger.LogInformation(
            "会员余额扣减成功: {MemberId}, 金额: {Amount:F2}, 原因: {Reason}",
            member.Id,
            command.Amount,
            command.Reason
        );

        return @event;
    }

    [Transactional]
    public async Task<Result> Handle(
        DeductBalance command,
        IDocumentSession session,
        ILogger<DeductBalanceHandler> logger,
        CancellationToken ct = default
    )
    {
        await HandleWithCascading(command, session, logger, ct);
        return Result.Success();
    }
}
