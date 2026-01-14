using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Core;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 充值余额命令处理器
/// Top up balance command handler
/// </summary>
public sealed class TopUpBalanceHandler
{
    [Transactional]
    public async Task<BalanceToppedUp> Handle(
        TopUpBalance command,
        IDocumentSession session,
        ILogger<TopUpBalanceHandler> logger,
        CancellationToken ct = default)
    {
        // 1. 加载会员
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            throw new MembersDomainException(MemberErrorDescriptors.MemberNotFound(command.MemberId));

        var oldBalance = member.Balance;

        // 2. 调用领域方法（可能抛出异常）
        member.TopUp(command.Amount);

        // 3. 持久化（[Transactional] 特性会自动调用 SaveChangesAsync）
        session.Store(member);

        logger.LogInformation(
            "会员充值成功: {MemberId}, 金额: {Amount:F2}, 余额: {Balance:F2}",
            member.Id,
            command.Amount,
            member.Balance
        );

        // 4. 返回级联消息（Wolverine 会自动发布）
        return new BalanceToppedUp(
            member.Id,
            command.Amount,
            oldBalance,
            member.Balance,
            DateTimeOffset.UtcNow
        );
    }
}
