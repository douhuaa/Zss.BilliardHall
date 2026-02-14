namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令处理器
/// 职责：业务规则与一致性判断
/// 垂直切片原则：所有业务逻辑都在这个 Handler 中，不依赖横向 Service
/// </summary>
public class CreateMemberCommandHandler(IDocumentSession session): ICommandHandler<CreateMemberCommand, Guid>
{
    public Task<Guid> Handle(CreateMemberCommand command)
    {
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

        return Task.FromResult(member.Id);
    }
}
