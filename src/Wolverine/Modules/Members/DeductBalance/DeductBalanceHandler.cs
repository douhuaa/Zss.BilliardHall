using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Behaviors;
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
        {
            var error = MemberErrorDescriptors.MemberNotFound(command.MemberId);
            logger.LogWarning(
                "扣减余额失败: {ErrorCode}, {Message}",
                error.Code,
                error.FormatMessage()
            );
            return (Result.Fail(error.FormatMessage(), error.Code), null);
        }

        var oldBalance = member.Balance;

        // 使用领域结果模式处理业务规则
        var domainResult = member.Deduct(command.Amount);
        if (!domainResult.IsSuccess)
        {
            // 使用统一的异常处理器转换 DomainResult
            var (result, _) = DomainExceptionHandler.ToResult(domainResult, logger);
            return (result, null);
        }

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
}
