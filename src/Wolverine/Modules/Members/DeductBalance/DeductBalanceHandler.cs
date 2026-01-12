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
    public async Task<(Result Result, BalanceDeducted? Event)> Handle(
        DeductBalance command,
        IDocumentSession session,
        ILogger<DeductBalanceHandler> logger,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            return (Result.Fail("会员不存在"), null);

        // 扣减（会检查余额是否足够）
        try
        {
            var oldBalance = member.Balance;
            member.Deduct(command.Amount);

            session.Store(member);

            // 返回级联消息（Wolverine 会自动发布）
            var @event = new BalanceDeducted(
                member.Id,
                command.Amount,
                oldBalance,
                member.Balance,
                command.Reason,
                DateTimeOffset.UtcNow
            );

            logger.LogInformation(
                "会员余额扣减成功: {MemberId}, 金额: {Amount:F2}, 原因: {Reason}",
                member.Id,
                command.Amount,
                command.Reason
            );

            return (Result.Success(), @event);
        }
        catch (InvalidOperationException ex)
        {
            return (Result.Fail(ex.Message), null);
        }
    }
}
