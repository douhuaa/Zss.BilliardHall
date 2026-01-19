namespace Zss.BilliardHall.Modules.Orders.Features.CreateOrder;

/// <summary>
/// 创建订单命令处理器
/// 职责：业务规则与一致性判断
/// 
/// 重要：不应依赖 Members 模块的查询接口来验证会员
/// 正确做法：
/// 1. 发布命令/事件让 Members 模块验证
/// 2. 或在 Orders 模块维护会员状态的本地副本（通过订阅 Members 模块的领域事件）
/// </summary>
public class CreateOrderCommandHandler
{
    // ❌ 错误示例：
    // private readonly IMemberQueries _memberQueries; // 不应依赖其他模块的查询
    
    // ✅ 正确示例：
    // private readonly IMessageBus _bus; // 用于发布命令/事件
    // private readonly IDocumentSession _session; // 用于持久化
    
    public async Task<Guid> Handle(CreateOrderCommand command)
    {
        // 1. 验证业务规则（台号是否可用等）
        // 2. 验证会员状态（通过本地副本或发布验证命令）
        // 3. 创建订单聚合根
        // 4. 持久化
        // 5. 发布 OrderCreated 事件
        
        // 示例代码（未实现）
        var orderId = Guid.NewGuid();
        
        // TODO: 实现业务逻辑
        await Task.CompletedTask;
        
        return orderId;
    }
}
