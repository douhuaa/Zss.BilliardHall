# 如何实现跨模块通信

> 📘 **Guide - 基于 ADR-0001 的操作指南**  
> **对应 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)  
> **最后更新**：2026-01-26

---

## 目的

本指南解释如何在模块间实现符合架构约束的通信，确保模块隔离的同时满足业务需求。

---

## 前置条件

- 理解 ADR-0001 中的模块隔离规则
- 熟悉领域事件和契约的概念
- 已设置开发环境

---

## 通信方式对比

| 方式 | 场景 | 是否允许 | 性能 | 一致性 |
|------|------|---------|------|--------|
| **领域事件** | 通知其他模块 | ✅ 允许 | 异步 | 最终一致 |
| **契约查询** | 显示其他模块数据 | ✅ 允许 | 同步 | 强一致 |
| **原始类型** | 保存关联 ID | ✅ 允许 | N/A | N/A |
| **直接引用** | 任何 | ❌ 禁止 | - | - |

---

## 方式一：使用领域事件（异步通知）

### 场景

当模块 A 发生某个业务事件，需要通知模块 B 执行相应操作。

**示例**：订单创建后，需要通知库存模块扣减库存。

### 步骤

#### 1. 在源模块定义领域事件

```csharp
// Modules/Orders/Domain/Events/OrderCreatedEvent.cs
namespace Zss.BilliardHall.Modules.Orders.Domain.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid MemberId,
    List<OrderItem> Items,
    DateTime CreatedAt
) : IDomainEvent;
```

#### 2. 在领域模型中发布事件

```csharp
// Modules/Orders/Domain/Order.cs
public class Order : AggregateRoot
{
    public static Order Create(Guid memberId, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Items = items,
            CreatedAt = DateTime.UtcNow
        };
        
        // 发布领域事件
        order.AddDomainEvent(new OrderCreatedEvent(
            order.Id,
            order.MemberId,
            order.Items,
            order.CreatedAt
        ));
        
        return order;
    }
}
```

#### 3. 在目标模块订阅事件

```csharp
// Modules/Inventory/EventHandlers/OrderCreatedEventHandler.cs
namespace Zss.BilliardHall.Modules.Inventory.EventHandlers;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IInventoryRepository _repository;
    
    public async Task Handle(OrderCreatedEvent @event)
    {
        // 处理库存扣减
        foreach (var item in @event.Items)
        {
            await _repository.ReserveStock(item.ProductId, item.Quantity);
        }
    }
}
```

### 注意事项

- ✅ 事件应该描述"已发生的事实"（过去式）
- ✅ 事件数据应该是不可变的（使用 record）
- ✅ 订阅者不应该返回结果给发布者
- ❌ 不要在事件中包含领域对象（仅原始类型和 DTO）

---

## 方式二：使用契约查询（同步读取）

### 场景

模块 A 需要显示模块 B 的数据，但不修改它。

**示例**：订单详情页面需要显示会员信息。

### 步骤

#### 1. 在目标模块定义契约

```csharp
// Modules/Members/Contracts/MemberDto.cs
namespace Zss.BilliardHall.Modules.Members.Contracts;

public record MemberDto(
    Guid Id,
    string Name,
    string Email,
    MembershipLevel Level
);
```

#### 2. 在目标模块提供查询

```csharp
// Modules/Members/Queries/GetMemberDetails.cs
namespace Zss.BilliardHall.Modules.Members.Queries;

public record GetMemberDetails(Guid MemberId) : IQuery<MemberDto>;

public class GetMemberDetailsHandler : IQueryHandler<GetMemberDetails, MemberDto>
{
    private readonly IMemberRepository _repository;
    
    public async Task<MemberDto> Handle(GetMemberDetails query)
    {
        var member = await _repository.GetByIdAsync(query.MemberId);
        
        return new MemberDto(
            member.Id,
            member.Name,
            member.Email,
            member.Level
        );
    }
}
```

#### 3. 在源模块使用契约

```csharp
// Modules/Orders/Queries/GetOrderDetails.cs
namespace Zss.BilliardHall.Modules.Orders.Queries;

using Zss.BilliardHall.Modules.Members.Contracts; // ✅ 引用契约

public class GetOrderDetailsHandler : IQueryHandler<GetOrderDetails, OrderDetailsDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator; // 用于跨模块查询
    
    public async Task<OrderDetailsDto> Handle(GetOrderDetails query)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId);
        
        // 查询会员信息
        var memberInfo = await _mediator.Send(
            new GetMemberDetails(order.MemberId)
        );
        
        return new OrderDetailsDto
        {
            OrderId = order.Id,
            MemberName = memberInfo.Name,  // ✅ 使用契约数据
            Items = order.Items
        };
    }
}
```

### 注意事项

- ✅ 契约应该是只读的
- ✅ 契约应该放在独立的 Contracts 命名空间
- ✅ 可以跨模块查询契约
- ❌ 不要在 Command Handler 中用契约做业务决策
- ❌ 不要修改契约返回的数据

---

## 方式三：使用原始类型（保存关联）

### 场景

模块 A 需要记录与模块 B 的关联关系，但不需要 B 的详细信息。

**示例**：订单需要记录是哪个会员创建的。

### 步骤

#### 1. 在源模块保存 ID

```csharp
// Modules/Orders/Domain/Order.cs
namespace Zss.BilliardHall.Modules.Orders.Domain;

public class Order : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }  // ✅ 仅保存 ID（原始类型）
    public List<OrderItem> Items { get; private set; }
    
    public Order(Guid memberId, List<OrderItem> items)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;  // ✅ 原始类型
        Items = items;
    }
}
```

#### 2. 需要详细信息时再查询

```csharp
// 在需要会员详细信息时，使用契约查询
var memberInfo = await _mediator.Send(new GetMemberDetails(order.MemberId));
```

### 注意事项

- ✅ 仅保存 Guid、string、int 等原始类型
- ✅ 在需要时通过契约查询详细信息
- ❌ 不要保存其他模块的领域对象引用
- ❌ 不要保存复杂对象

---

## 常见错误

### ❌ 错误：直接引用其他模块的领域对象

```csharp
using Zss.BilliardHall.Modules.Members.Domain;  // ❌ 引用其他模块的 Domain

public class Order
{
    public Member Member { get; set; }  // ❌ 直接保存领域对象
}
```

**正确做法**：保存 ID 或使用契约

```csharp
using Zss.BilliardHall.Modules.Members.Contracts;  // ✅ 引用契约

public class Order
{
    public Guid MemberId { get; set; }  // ✅ 保存 ID
}

// 查询时使用契约
var memberInfo = await _mediator.Send(new GetMemberDetails(order.MemberId));
```

---

### ❌ 错误：同步调用其他模块的 Command

```csharp
// ❌ 禁止：同步调用其他模块的命令
await _mediator.Send(new CreateMemberCommand(...));
```

**正确做法**：使用领域事件

```csharp
// ✅ 发布事件，让其他模块异步处理
order.AddDomainEvent(new OrderCreatedEvent(...));
```

---

### ❌ 错误：在 Command Handler 中使用契约做业务决策

```csharp
// ❌ 错误：在 Command 中查询契约做业务决策
public class CreateOrderHandler
{
    public async Task<Guid> Handle(CreateOrder command)
    {
        var memberInfo = await _mediator.Send(
            new GetMemberDetails(command.MemberId)
        );
        
        if (memberInfo.Level == MembershipLevel.VIP)  // ❌ 用契约做业务决策
        {
            // 应用折扣...
        }
    }
}
```

**正确做法**：将必要信息通过命令传入，或加载完整领域模型

```csharp
// ✅ 方案 1：命令中包含必要信息
public record CreateOrder(
    Guid MemberId,
    MembershipLevel MemberLevel,  // ✅ 命令中包含
    List<OrderItem> Items
) : ICommand<Guid>;

// ✅ 方案 2：如果需要完整业务逻辑，考虑是否应该在同一模块
```

---

## 决策树：选择合适的通信方式

```
需要跨模块通信？
├─ 是否需要通知其他模块某事已发生？
│  └─ 是 → 使用领域事件（异步）
│
├─ 是否需要显示其他模块的数据？
│  └─ 是 → 使用契约查询（同步，只读）
│
└─ 仅需要记录关联关系？
   └─ 是 → 使用原始类型（ID）
```

---

## 验证

运行架构测试验证您的实现：

```bash
# 测试模块隔离
dotnet test --filter "FullyQualifiedName~ADR_0001"

# 查看详细错误
./scripts/verify-all.sh
```

---

## 相关文档

- [ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 2.2 节
- [ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [架构 FAQ](../faqs/architecture-faq.md) - 模块通信相关问题
- [架构指南](../architecture-guide.md)

---

**维护**：Tech Lead  
**最后审核**：2026-01-26  
**状态**：✅ Active
