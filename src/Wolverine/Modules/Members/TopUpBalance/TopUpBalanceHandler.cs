using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 充值余额命令处理器
/// Top up balance command handler
/// </summary>
public sealed class TopUpBalanceHandler
{
    [Transactional]
    public async Task<(Result Result, BalanceToppedUp? Event)> Handle(
        TopUpBalance command,
        IDocumentSession session,
        ILogger<TopUpBalanceHandler> logger,
        CancellationToken ct = default)
    {
        // 1. 加载会员
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            return (Result.Fail("会员不存在", "Member.NotFound"), null);

        var oldBalance = member.Balance;

        // 2. 调用领域方法（异常会由 DomainExceptionMiddleware 处理）
        member.TopUp(command.Amount);

        // 3. 持久化（[Transactional] 特性会自动调用 SaveChangesAsync）
        session.Store(member);

        // 4. 返回级联消息（Wolverine 会自动发布）
        var @event = new BalanceToppedUp(
            member.Id,
            command.Amount,
            oldBalance,
            member.Balance,
            DateTimeOffset.UtcNow
        );

        logger.LogInformation(
            "会员充值成功: {MemberId}, 金额: {Amount:F2}, 余额: {Balance:F2}",
            member.Id,
            command.Amount,
            member.Balance
        );

        return (Result.Success(), @event);
    }
}
