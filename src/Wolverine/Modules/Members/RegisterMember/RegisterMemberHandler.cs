using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.BuildingBlocks.Exceptions;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员命令处理器
/// Register member command handler
/// </summary>
public sealed class RegisterMemberHandler
{
    [Transactional]
    public async Task<(Guid MemberId, MemberRegistered Event)> HandleWithCascading(
        RegisterMember command,
        IDocumentSession session,
        ILogger<RegisterMemberHandler> logger,
        CancellationToken ct = default
    )
    {
        var exists = await session.Query<Member>().AnyAsync(m => m.Phone == command.Phone, ct);

        // 这句非常关键：
        // 校验发生在 Handler（对的）
        // 失败语义仍然是 DomainException（对的）
        // Aggregate 不被污染（对的）
        // 👉 这是Vertical Slice 下“跨聚合规则”的标准位置。
        if (exists)
            throw new DomainException(MemberErrorCodes.DuplicatePhone);

        // 2. 创建会员
        // TODO: Implement password hashing and storage when authentication module is ready
        var member = Member.Register(command.Name, command.Phone, command.Email);

        // 3. 持久化（[Transactional] 特性会自动调用 SaveChangesAsync）
        session.Store(member);

        // 4. 返回级联消息（Wolverine 会自动发布）
        var @event = new MemberRegistered(member.Id, member.Name, member.Phone);

        logger.LogInformation("会员注册成功: {MemberId}, 手机号: {Phone}", member.Id, member.Phone);

        // ✅ 成功 = 返回结果 + 事件 ❌ 失败 = DomainException
        return (member.Id, @event);
    }

    [Transactional]
    public async Task<Result<Guid>> Handle(
        RegisterMember command,
        IDocumentSession session,
        ILogger<RegisterMemberHandler> logger,
        CancellationToken ct = default
    )
    {
        var (memberId, _) = await HandleWithCascading(command, session, logger, ct);
        return Result.Success(memberId);
    }
}
