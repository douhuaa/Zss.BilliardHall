using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Core;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.DeductBalance;

/// <summary>
/// 扣减余额命令处理器
/// Deduct balance command handler
/// </summary>
public sealed class DeductBalanceHandler
{
    [Transactional]
    public async Task<BalanceDeducted> Handle(
        DeductBalance command,
        IDocumentSession session,
        ILogger<DeductBalanceHandler> logger,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            throw new MembersDomainException(MemberErrorDescriptors.MemberNotFound(command.MemberId));

        var oldBalance = member.Balance;

        // 使用领域方法（可能抛出异常）
        member.Deduct(command.Amount);

        session.Store(member);

        logger.LogInformation(
            "会员余额扣减成功: {MemberId}, 金额: {Amount:F2}, 原因: {Reason}",
            member.Id,
            command.Amount,
            command.Reason
        );

        // 返回级联消息（Wolverine 会自动发布）
        return new BalanceDeducted(
            member.Id,
            command.Amount,
            oldBalance,
            member.Balance,
            command.Reason,
            DateTimeOffset.UtcNow
        );
    }
}
