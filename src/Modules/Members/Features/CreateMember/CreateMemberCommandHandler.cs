using Zss.BilliardHall.Modules.Members.Exceptions;

namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令处理器
/// 职责：业务规则与一致性判断
/// 垂直切片原则：所有业务逻辑都在这个 Handler 中，不依赖横向 Service
/// </summary>
public class CreateMemberCommandHandler(IDocumentSession session) : ICommandHandler<CreateMemberCommand, Guid>
{
    public async Task<Guid> Handle(CreateMemberCommand command)
    {
        // 创建前检查邮箱是否重复
        var emailExists = await session.Query<Member>()
            .AnyAsync(x => x.Email == command.Email);
        if (emailExists)
            throw new MemberEmailAlreadyExistsException();

        // 创建前检查手机号是否重复（手机号为可选字段，仅在提供时检查）
        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneExists = await session.Query<Member>()
                .AnyAsync(x => x.PhoneNumber == command.PhoneNumber);
            if (phoneExists)
                throw new MemberPhoneNumberAlreadyExistsException();
        }

        var member = new Member
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow
        };

        session.Store(member);

        // ✅ IntegrateWithWolverine() + AutoApplyTransactions() 会在消息 pipeline 的事务上下文里自动提交

        return member.Id;
    }
}
