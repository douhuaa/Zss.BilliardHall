using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.AwardPoints;

/// <summary>
/// 赠送积分命令处理器
/// Award points command handler
/// </summary>
public sealed class AwardPointsHandler
{
    [Transactional]
    public async Task<PointsAwarded> Handle(
        AwardPoints command,
        IDocumentSession session,
        ILogger<AwardPointsHandler> logger,
        CancellationToken ct = default
    )
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            throw MembersDomainErrors.NotFound();

        member.AwardPoints(command.Points);

        session.Store(member);

        // 返回级联消息（Wolverine 会自动发布）
        var @event = new PointsAwarded(
            member.Id,
            command.Points,
            command.Reason,
            DateTimeOffset.UtcNow
        );

        logger.LogInformation(
            "积分赠送成功: {MemberId}, 积分: {Points}, 原因: {Reason}",
            member.Id,
            command.Points,
            command.Reason
        );

        return @event;
    }
}
