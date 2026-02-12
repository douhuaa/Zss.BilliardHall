using Marten;

namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令处理器
/// 职责：业务规则与一致性判断
/// 垂直切片原则：所有业务逻辑都在这个 Handler 中，不依赖横向 Service
/// </summary>
public class CreateMemberCommandHandler
{
    public static async Task<Guid> Handle(CreateMemberCommand command, IDocumentSession session)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(member);
        await session.SaveChangesAsync();

        return member.Id;
    }
}
