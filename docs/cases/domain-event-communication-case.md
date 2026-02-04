# 案例：领域事件通信模式

> ⚠️ **无裁决力声明**：本文档为实践案例说明，不具备架构裁决权。所有架构决策以 [ADR 文档](../adr/) 为准。

## Metadata

- 难度：🟡 中等
- 级别: Core
- 相关 ADR：[ADR-001](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md), [ADR-005](../adr/constitutional/ADR-005-Application-Interaction-Model-Final.md)
- 作者：@douhuaa
- 日期：2026-01-27
- 标签：模块化, 领域事件, 异步通信, 模块隔离

---

## 适用场景

当一个模块需要通知其他模块某个业务事件已发生，但**不需要知道**谁会处理这个事件，也**不关心**处理结果时，使用领域事件通信模式。

**典型场景**：
- 订单创建后，通知计费模块生成账单
- 会员注册后，通知积分模块初始化积分账户
- 订单取消后，通知库存模块释放库存

---

## 背景

在模块化单体架构中，模块之间必须保持隔离（根据 ADR-001）。直接调用其他模块的 Handler 或共享领域对象会破坏模块边界，导致紧耦合。

领域事件提供了一种**异步、解耦**的通信方式：
- **发布者**：只负责发布事件，不知道谁会订阅
- **订阅者**：监听感兴趣的事件，独立处理业务逻辑
- **事件总线**：负责路由事件到订阅者

---

## 解决方案

### 架构设计

```
# 模块间事件通信流程
Orders Module                    Billing Module
┌────────────────┐              ┌────────────────┐
│ CreateOrder    │              │ Event Handler  │
│ Handler        │              │                │
│                │              │                │
│ 1. Create Order│              │ 3. Generate    │
│ 2. Publish     │─────Event───▶│    Invoice     │
│    Event       │              │                │
└────────────────┘              └────────────────┘
        │
        ▼
    EventBus
```

**关键要素**：
1. **领域事件定义**：在 BuildingBlocks 中定义事件契约
2. **事件发布**：在 Command Handler 中发布事件
3. **事件订阅**：在目标模块中创建事件处理器

---

### 代码示例

#### 步骤 1：定义领域事件（BuildingBlocks）

```csharp
// src/BuildingBlocks/Zss.BilliardHall.BuildingBlocks/Events/OrderCreatedEvent.cs
namespace Zss.BilliardHall.BuildingBlocks.Events;

/// <summary>
/// 订单创建事件
/// 根据 ADR-001，事件是模块间通信的合规方式
/// </summary>
public sealed record OrderCreatedEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public Guid MemberId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

**要点**：
- 使用 `record` 类型（不可变）
- 包含必要的数据，但**不暴露领域对象**
- 定义在 BuildingBlocks 中，可被多个模块引用

#### 步骤 2：在 Handler 中发布事件

```csharp
// src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs
namespace Zss.BilliardHall.Modules.Orders.UseCases.CreateOrder;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Guid>
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateOrderHandler(
        IOrderRepository repository,
        IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(CreateOrder command)
    {
        // 1. 创建订单领域对象
        var order = new Order(
            memberId: command.MemberId,
            items: command.Items
        );

        // 2. 保存到仓储
        await _repository.SaveAsync(order);

        // 3. 发布领域事件
        await _eventBus.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            MemberId = order.MemberId,
            TotalAmount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow
        });

        // 4. 返回 ID（根据 ADR-005）
        return order.Id;
    }
}
```

**要点**：
- Command Handler 只返回 ID，不返回业务数据（ADR-005）
- 事件发布在业务逻辑完成**之后**
- 事件是**"已发生的事实"**，用过去式命名（`OrderCreated`）

#### 步骤 3：在目标模块中订阅事件

```csharp
// src/Modules/Billing/EventHandlers/OrderCreatedEventHandler.cs
namespace Zss.BilliardHall.Modules.Billing.EventHandlers;

public sealed class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IInvoiceRepository _repository;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IInvoiceRepository repository,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation(
            "Processing OrderCreated event for Order {OrderId}",
            @event.OrderId);

        // 1. 创建账单领域对象
        var invoice = new Invoice(
            orderId: @event.OrderId,
            memberId: @event.MemberId,
            amount: @event.TotalAmount
        );

        // 2. 保存账单
        await _repository.SaveAsync(invoice);

        _logger.LogInformation(
            "Invoice {InvoiceId} generated for Order {OrderId}",
            invoice.Id,
            @event.OrderId);
    }
}
```

**要点**：
- 事件处理器在**独立的模块**中
- 使用事件中的数据，而非直接访问 Orders 模块
- 记录日志用于追踪事件流

#### 步骤 4：注册事件处理器

```csharp
// src/Modules/Billing/BillingModule.cs
public static class BillingModule
{
    public static IServiceCollection AddBillingModule(
        this IServiceCollection services)
    {
        // 注册事件处理器
        services.AddTransient<IEventHandler<OrderCreatedEvent>, 
                               OrderCreatedEventHandler>();

        // 其他服务注册...
        
        return services;
    }
}
```

---

### 测试验证

#### 单元测试：验证事件发布

```csharp
// src/tests/Modules.Orders.Tests/UseCases/CreateOrder/CreateOrderHandlerTests.cs
public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_PublishesOrderCreatedEvent()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var handler = new CreateOrderHandler(repository, eventBus);
        
        var command = new CreateOrder(
            MemberId: Guid.NewGuid(),
            Items: new[] { new OrderItem("product1", 2) }
        );

        // Act
        var orderId = await handler.Handle(command);

        // Assert
        await eventBus.Received(1).PublishAsync(
            Arg.Is<OrderCreatedEvent>(e => 
                e.OrderId == orderId && 
                e.MemberId == command.MemberId)
        );
    }
}
```

#### 集成测试：验证端到端事件流

```csharp
// src/tests/IntegrationTests/Events/OrderCreatedEventFlowTests.cs
[Collection("Integration")]
public class OrderCreatedEventFlowTests
{
    private readonly IntegrationTestFixture _fixture;

    public OrderCreatedEventFlowTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OrderCreated_TriggersInvoiceGeneration()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var orderCommand = new CreateOrder(
            MemberId: memberId,
            Items: new[] { new OrderItem("product1", 2, 100m) }
        );

        // Act - 创建订单
        var orderId = await _fixture.SendAsync(orderCommand);

        // Wait for event processing
        await Task.Delay(1000);

        // Assert - 验证账单已生成
        var invoices = await _fixture.QueryAsync(
            new GetInvoicesByOrder(orderId)
        );

        invoices.Should().ContainSingle()
            .Which.OrderId.Should().Be(orderId);
    }
}
```

---

## 常见陷阱

### ❌ 陷阱 1：在事件中传递领域对象

```csharp
// ❌ 错误：暴露领域对象
public record OrderCreatedEvent
{
    public Order Order { get; init; }  // 违反模块隔离
}
```

**问题**：
- 其他模块依赖 Orders 模块的领域对象
- 破坏模块边界

**正确做法**：
```csharp
// ✅ 正确：只传递必要数据
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid MemberId { get; init; }
    public decimal TotalAmount { get; init; }
}
```

### ❌ 陷阱 2：事件处理器中修改发布者的状态

```csharp
// ❌ 错误：事件处理器不应该反向修改订单
public async Task HandleAsync(OrderCreatedEvent @event)
{
    // 生成账单
    var invoice = new Invoice(...);
    await _repository.SaveAsync(invoice);
    
    // ❌ 不要尝试修改订单状态
    await _orderRepository.UpdateOrderStatusAsync(@event.OrderId, "Invoiced");
}
```

**问题**：
- 创建了反向依赖
- 违反单向事件流原则

**正确做法**：
- 如果需要通知订单，发布**新的事件**（如 `InvoiceGeneratedEvent`）
- Orders 模块订阅该事件并自行更新状态

### ❌ 陷阱 3：同步等待事件处理结果

```csharp
// ❌ 错误：等待事件处理完成
var orderId = await handler.Handle(command);
await eventBus.PublishAsync(new OrderCreatedEvent { ... });

// ❌ 不要这样做
while (!IsInvoiceGenerated(orderId))
{
    await Task.Delay(100);
}
```

**问题**：
- 违反异步通信原则
- 引入隐式依赖

**正确做法**：
- 事件是**"发完即忘"**（Fire and Forget）
- 如果需要知道处理结果，使用查询或订阅回执事件

---

## 最佳实践

### ✅ 实践 1：事件命名使用过去式

```csharp
// ✅ 好的命名
OrderCreatedEvent
MemberRegisteredEvent
PaymentCompletedEvent

// ❌ 不好的命名
CreateOrderEvent      // 这是命令，不是事件
OrderCreateEvent      // 时态不对
```

### ✅ 实践 2：保持事件小而专注

```csharp
// ✅ 好的事件设计
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid MemberId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ❌ 过于复杂的事件
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public CompleteOrderDetails Details { get; init; }  // 太多细节
    public List<OrderItem> Items { get; init; }         // 可能不需要
    public ShippingAddress Address { get; init; }       // 可能不需要
}
```

### ✅ 实践 3：事件处理器的幂等性

```csharp
public async Task HandleAsync(OrderCreatedEvent @event)
{
    // ✅ 检查是否已处理过
    var existingInvoice = await _repository
        .GetByOrderIdAsync(@event.OrderId);
    
    if (existingInvoice != null)
    {
        _logger.LogInformation(
            "Invoice already exists for Order {OrderId}, skipping",
            @event.OrderId);
        return;
    }

    // 继续处理...
}
```

**原因**：
- 事件可能因为重试被多次投递
- 幂等性确保重复处理不会产生副作用

---

## 架构合规检查清单

根据 ADR-001 和 ADR-005，确认：

- [ ] 事件定义在 BuildingBlocks，不在模块内
- [ ] 事件不包含领域对象引用
- [ ] 发布者不知道订阅者是谁
- [ ] 订阅者不修改发布者的状态
- [ ] Command Handler 只返回 ID，不返回业务数据
- [ ] 事件处理器具有幂等性
- [ ] 架构测试通过（无跨模块直接依赖）

---

## 参考资料

- [ADR-001：模块化单体与垂直切片架构](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md) - 第 2.2 节：模块通信规则
- [ADR-005：应用内交互模型与执行边界](../adr/constitutional/ADR-005-Application-Interaction-Model-Final.md) - 第 2.1 节：Command Handler 规则
- [模块化架构 FAQ](../faqs/architecture-faq.md) - Q: 模块间如何通信？
- [跨模块通信指南](../guides/cross-module-communication.md)

---

## 相关案例

- [契约查询模式](contract-query-pattern-case.md) - 跨模块同步查询数据
- [Handler 单元测试](handler-unit-testing-case.md) - 测试事件发布逻辑

---

**维护**：Tech Lead  
**状态**：✅ Active  
**审核**: 已通过架构委员会审查（2026-01-27）  
**最后更新**：2026-01-27
