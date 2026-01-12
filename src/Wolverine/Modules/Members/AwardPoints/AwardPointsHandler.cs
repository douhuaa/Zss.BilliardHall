using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;
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
    public async Task<Result> Handle(
        AwardPoints command,
        IDocumentSession session,
        IMessageBus bus,
        ILogger<AwardPointsHandler> logger,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
            return Result.Fail("会员不存在");

        try
        {
            member.AwardPoints(command.Points);

            session.Store(member);

            await bus.PublishAsync(new PointsAwarded(
                member.Id,
                command.Points,
                command.Reason,
                DateTimeOffset.UtcNow
            ));

            logger.LogInformation(
                "积分赠送成功: {MemberId}, 积分: {Points}, 原因: {Reason}",
                member.Id,
                command.Points,
                command.Reason
            );

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
