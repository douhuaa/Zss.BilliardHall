using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.Modules.Members.Events;

namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员命令处理器
/// Register member command handler
/// </summary>
public sealed class RegisterMemberHandler
{
    [Transactional]
    public async Task<(Result<Guid> Result, MemberRegistered? Event)> Handle(
        RegisterMember command,
        IDocumentSession session,
        ILogger<RegisterMemberHandler> logger,
        CancellationToken ct = default)
    {
        // 1. 检查手机号是否已注册
        var existing = await session
            .Query<Member>()
            .FirstOrDefaultAsync(m => m.Phone == command.Phone, ct);

        if (existing != null)
            return (Result.Fail<Guid>("手机号已注册"), null);

        // 2. 创建会员
        // TODO: Implement password hashing and storage when authentication module is ready
        // Password is accepted in the command but not stored yet - authentication will be handled by OpenIddict
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Phone = command.Phone,
            Email = command.Email,
            Tier = MemberTier.Regular,
            Balance = 0,
            Points = 0,
            RegisteredAt = DateTimeOffset.UtcNow
        };

        // 3. 持久化（[Transactional] 特性会自动调用 SaveChangesAsync）
        session.Store(member);

        // 4. 返回级联消息（Wolverine 会自动发布）
        var @event = new MemberRegistered(
            member.Id,
            member.Name,
            member.Phone,
            member.RegisteredAt
        );

        logger.LogInformation(
            "会员注册成功: {MemberId}, 手机号: {Phone}",
            member.Id,
            member.Phone
        );

        return (Result.Success(member.Id), @event);
    }
}
