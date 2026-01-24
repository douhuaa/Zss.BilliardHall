# 后端开发指令

## 适用场景：后端/业务逻辑开发

在协助后端开发时，在 `base.instructions.md` 的基础上应用这些额外约束。

## ⚖️ 权威提醒

所有后端开发约束基于以下 **ADR 正文**：

- `ADR-0001-modular-monolith-vertical-slice-architecture.md` - 模块隔离和垂直切片
- `ADR-0005-Application-Interaction-Model-Final.md` - Handler 规则和 CQRS

引用规则时，必须以 ADR 正文为准，Prompt 文件仅为辅助理解。

## 垂直切片组织

每个业务用例必须组织为完整的垂直切片：

```
UseCases/
  CreateOrder/
    CreateOrder.cs              ← 命令/查询
    CreateOrderHandler.cs        ← Handler（此用例的权威）
    CreateOrderEndpoint.cs       ← 可选：HTTP 适配器
    CreateOrderTests.cs          ← 测试
```

**绝不建议**：

- ❌ 水平 Service 层（如 `OrderService`）
- ❌ 跨用例共享业务逻辑
- ❌ 包含业务逻辑的通用 `Manager` 或 `Helper` 类

## Handler 规则（ADR-0005）

### Command Handler

- 必须返回 `void` 或仅返回 ID（Guid、int、string）
- 不得返回业务数据（使用单独的 Query）
- 不得依赖契约（DTO）进行业务决策
- 必须加载领域模型、执行业务逻辑、保存状态
- 可以发布领域事件

**正确的 Command Handler**：

```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrder>
{
    public async Task<Guid> Handle(CreateOrder command)
    {
        // ✅ 加载/创建聚合
        var order = new Order(command.MemberId, command.Items);
        
        // ✅ 执行业务逻辑（在领域模型中）
        order.Calculate();
        
        // ✅ 保存
        await _repository.SaveAsync(order);
        
        // ✅ 发布事件（可选）
        await _eventBus.Publish(new OrderCreated(order.Id));
        
        return order.Id;
    }
}
```

**必须阻止的模式**：

```csharp
// ❌ Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command) { ... }

// ❌ Command Handler 依赖契约
var memberDto = await _queryBus.Send(new GetMemberById(...));
if (memberDto.Balance > 1000) { ... } // ❌ 基于 DTO 的业务决策
```

### Query Handler

- 必须返回契约（DTO）
- 不得修改状态
- 不得发布事件
- 可以优化读取性能
- 可以跨模块边界查询（通过契约）

## Endpoint 规则

Endpoint 必须是薄适配器：

```csharp
public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/orders", async (
            CreateOrderRequest request, 
            IMessageBus bus) =>
        {
            // ✅ 映射到命令
            var command = new CreateOrder(request.MemberId, request.Items);
            
            // ✅ 委托给 Handler
            var orderId = await bus.InvokeAsync(command);
            
            // ✅ 返回 HTTP 响应
            return Results.Created($"/orders/{orderId}", orderId);
        });
    }
}
```

**Endpoint 中绝不允许**：

- ❌ 业务逻辑或验证
- ❌ 直接访问数据库
- ❌ 直接操作领域模型

## 模块通信

当一个模块需要来自另一个模块的数据/通知时：

### ✅ 使用：领域事件（异步）

```csharp
// 在 Orders 模块中
await _eventBus.Publish(new OrderCreated(orderId, memberId));

// 在 Members 模块中（订阅者）
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public async Task Handle(OrderCreated @event)
    {
        // 更新会员统计
    }
}
```

### ✅ 使用：契约查询

```csharp
// 查询另一个模块的数据
var memberDto = await _queryBus.Send(new GetMemberById(memberId));
// 使用 memberDto.Name、memberDto.Email 等（只读）
```

### ✅ 使用：原始类型

```csharp
// 只传递 ID
var orderId = Guid.NewGuid();
var command = new NotifyMember(memberId); // Guid，而非 Member 对象
```

### ❌ 禁止：直接引用

```csharp
// ❌ 永远不要引用其他模块的内部实现
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(id);
```

### ❌ 禁止：同步跨模块命令

```csharp
// ❌ 不要同步调用另一个模块的命令
await _commandBus.Send(new UpdateMemberStatistics(memberId));
```

## 领域模型指南

将业务逻辑放在领域模型中，而非 Handler 或 Service：

```csharp
// ✅ 正确：业务逻辑在领域模型中
public class Order
{
    public void ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new InvalidDiscountException();
        
        _discount = percentage;
        AddDomainEvent(new DiscountApplied(Id, percentage));
    }
}

// Handler 只是编排
public class ApplyDiscountHandler
{
    public async Task Handle(ApplyDiscount command)
    {
        var order = await _repository.GetByIdAsync(command.OrderId);
        order.ApplyDiscount(command.Percentage); // ✅ 逻辑在领域模型中
        await _repository.SaveAsync(order);
    }
}
```

## 何时建议什么

| 开发者说...             | 建议检查...                                           |
|---------------------|---------------------------------------------------|
| "我需要调用另一个模块的逻辑"     | ADR-0001（使用事件），`docs/copilot/adr-0001.prompts.md` |
| "我需要在模块间共享代码"       | 是技术性的（→ BuildingBlocks）还是业务性的（→ 重新思考设计）？          |
| "我需要从命令返回数据"        | ADR-0005（命令返回 ID，使用单独的查询）                         |
| "我需要使用另一个模块的数据进行验证" | 通过契约查询（只读），不要用于业务决策                               |

## 快速危险信号

发现以下情况时停止并警告：

- 🚩 在另一个模块中出现 `using Zss.BilliardHall.Modules.X`
- 🚩 模块中出现 `class OrderService` 或任何 `*Service`
- 🚩 Command Handler 返回 DTO
- 🚩 Query Handler 修改状态
- 🚩 Endpoint 中的业务逻辑
- �� 模块间共享的领域模型

## 参考

详细场景和示例：

- `docs/copilot/adr-0001.prompts.md` - 模块隔离
- `docs/copilot/adr-0005.prompts.md` - Handler 模式和 CQRS
