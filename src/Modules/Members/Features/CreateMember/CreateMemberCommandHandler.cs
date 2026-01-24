namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令处理器
/// 职责：业务规则与一致性判断
/// 垂直切片原则：所有业务逻辑都在这个 Handler 中，不依赖横向 Service
/// </summary>
public class CreateMemberCommandHandler
{
    // 在实际实现中，可能注入：
    // - IDocumentSession (Marten) 或其他持久化机制
    // - ILogger
    // - IMessageBus (用于发布领域事件)

    public async Task<Guid> Handle(CreateMemberCommand command)
    {
        // 1. 验证业务规则
        // 2. 创建聚合根
        // 3. 持久化
        // 4. 发布领域事件

        // 示例代码（未实现）
        var memberId = Guid.NewGuid();

        // TODO: 实现业务逻辑
        await Task.CompletedTask;

        return memberId;
    }
}
