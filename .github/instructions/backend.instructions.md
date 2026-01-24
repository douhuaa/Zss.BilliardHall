# 后端开发指令

> **⚠️ 权威声明**  
> 本文件所列规则仅作操作/辅导用，权威判据以 ADR 正文为准。  
> 若本文件与 ADR 正文存在分歧，请及时修订本文件，并以 ADR 正文为最终依据。

## 适用场景：后端/业务逻辑开发

在协助后端开发时，在 [`base.instructions.md`](./base.instructions.md) 的基础上应用这些额外约束。

---

## 🚨 高风险防御点（优先检查）

在开始任何后端开发前，必须警惕以下高风险模式：

### 🚩 跨模块违规（最高优先级）
```csharp
// ❌ 致命：跨模块直接引用
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(id);

// ❌ 致命：模块间共享领域模型
public class SharedCustomer { } // 被多个模块使用
```

### 🚩 Handler 违规
```csharp
// ❌ 严重：Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command) { ... }

// ❌ 严重：Query Handler 修改状态
public async Task<OrderDto> Handle(GetOrder query) {
    order.UpdateStatus(); // 不允许！
    await _repository.SaveAsync(order);
}
```

### 🚩 架构分层违规
```csharp
// ❌ 严重：Endpoint 包含业务逻辑
builder.MapPost("/orders", async (request, db) => {
    if (request.Total > 1000) { // 业务逻辑！
        // 应该在领域模型中
    }
});

// ❌ 严重：水平 Service 层
public class OrderService { } // 违反垂直切片
```

**相关 ADR**：
- [ADR-0001：模块隔离和垂直切片](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0005：Handler 规则和 CQRS](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0005：执行级别分类](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

---

## ⚖️ 权威依据

所有后端开发约束基于以下 **ADR 正文**：
- [ADR-0001-modular-monolith-vertical-slice-architecture.md](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 模块隔离和垂直切片
- [ADR-0005-Application-Interaction-Model-Final.md](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md) - Handler 规则和 CQRS

引用规则时，必须以 ADR 正文为准，Prompt 文件仅为辅助理解。

**执行级别参考**：
- Level 1（静态可执行）：[ADR-0005-Enforcement-Levels.md#level-1-静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)
- Level 2（语义半自动）：[ADR-0005-Enforcement-Levels.md#level-2-语义半自动](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-2-语义半自动semantic-semi-auto)
- Level 3（人工 Gate）：[ADR-0005-Enforcement-Levels.md#level-3-人工-gate](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-3-人工-gatemanual-gate)

---

## 垂直切片组织

每个业务用例必须组织为完整的垂直切片：

```
UseCases/
  CreateOrder/
    CreateOrder.cs              ← 命令/查询
    CreateOrderHandler.cs       ← Handler（此用例的权威）
    CreateOrderEndpoint.cs      ← 可选：HTTP 适配器
    CreateOrderTests.cs         ← 测试
```

**参考**：[ADR-0001：垂直切片组织](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md#垂直切片架构)

**绝不建议**（Level 1 自动阻止）：
- ❌ 水平 Service 层（如 `OrderService`）
- ❌ 跨用例共享业务逻辑
- ❌ 包含业务逻辑的通用 `Manager` 或 `Helper` 类

**执行级别**：[Level 1 - 静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)

---

## Handler 规则

**权威依据**：[ADR-0005：Handler 规范](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md#handler-规范)

### Command Handler

**规则**（Level 1 - 自动阻止）：
- 必须返回 `void` 或仅返回 ID（Guid、int、string）
- 不得返回业务数据（使用单独的 Query）
- 不得依赖契约（DTO）进行业务决策
- 必须加载领域模型、执行业务逻辑、保存状态
- 可以发布领域事件

**执行级别参考**：
- [ADR-0005.10 - Level 2](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-2-语义半自动semantic-semi-auto)：Command Handler 返回值检查

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

**规则**（Level 1 - 自动阻止）：
- 必须返回契约（DTO）
- 不得修改状态
- 不得发布事件
- 可以优化读取性能
- 可以跨模块边界查询（通过契约）

**参考**：[ADR-0005：Query Handler 规范](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md#query-handler-规范)

**执行级别**：[Level 1 - 静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)

---

## Endpoint 规则

**权威依据**：[ADR-0005：Endpoint 规范](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md#endpoint-规范)

Endpoint 必须是薄适配器（Level 2 - 语义半自动检查）：

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

**Endpoint 中绝不允许**（Level 2 - 需要人工审查）：
- ❌ 业务逻辑或验证
- ❌ 直接访问数据库
- ❌ 直接操作领域模型

**执行级别参考**：
- [ADR-0005.2 - Level 2](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-2-语义半自动semantic-semi-auto)：Endpoint 业务逻辑检查

---

## 模块通信

**权威依据**：[ADR-0001：模块通信规则](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md#模块通信)

当一个模块需要来自另一个模块的数据/通知时：

### ✅ 合规模式

#### 1. 使用领域事件（异步）

**执行级别**：[Level 1 - 静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)
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

#### 2. 使用契约查询

**执行级别**：[Level 1 - 静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)

```csharp
// 查询另一个模块的数据
var memberDto = await _queryBus.Send(new GetMemberById(memberId));
// 使用 memberDto.Name、memberDto.Email 等（只读）
```

#### 3. 使用原始类型

**执行级别**：[Level 1 - 静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)

```csharp
// 只传递 ID
var orderId = Guid.NewGuid();
var command = new NotifyMember(memberId); // Guid，而非 Member 对象
```

### ❌ 违规模式（自动阻止）

#### 禁止：直接引用

**执行级别**：[Level 1 - 静态可执行](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-1-静态可执行static-enforceable)

```csharp
// ❌ 永远不要引用其他模块的内部实现
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(id);
```

#### 禁止：同步跨模块命令

**执行级别**：[Level 2/3 - 需要人工判定](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-3-人工-gatemanual-gate)

```csharp
// ❌ 不要同步调用另一个模块的命令
await _commandBus.Send(new UpdateMemberStatistics(memberId));
```

**说明**：同步跨模块命令可能在特殊场景下获批，但需要：
- 提交 [ARCH-VIOLATION] PR
- 提供详细理由
- 架构委员会审批

**参考**：[ADR-0005-Enforcement-Levels.md#level-3-人工-gate](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md#level-3-人工-gatemanual-gate)

---

## 领域模型指南

**权威依据**：[ADR-0001：领域模型规范](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md#领域模型)

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

| 开发者说... | 建议检查... | 相关 ADR |
|-------------------|---------------------|----------|
| "我需要调用另一个模块的逻辑" | 使用领域事件（异步） | [ADR-0001](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)，[adr-0001.prompts.md](../../docs/copilot/adr-0001.prompts.md) |
| "我需要在模块间共享代码" | 技术性的→BuildingBlocks；业务性的→重新思考设计 | [ADR-0001](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) |
| "我需要从命令返回数据" | 命令返回 ID，使用单独的查询 | [ADR-0005](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md) |
| "我需要使用另一个模块的数据进行验证" | 通过契约查询（只读），不要用于业务决策 | [ADR-0005](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md) |

---

## 🚩 快速危险信号检查清单

发现以下情况时立即停止并警告：

### 致命违规（Level 1 - 自动阻止）
- 🚨 在另一个模块中出现 `using Zss.BilliardHall.Modules.X`
- 🚨 模块中出现 `class OrderService` 或任何 `*Service`
- 🚨 模块间共享的领域模型

### 严重违规（Level 2 - 需要审查）
- ⚠️ Command Handler 返回 DTO
- ⚠️ Query Handler 修改状态
- ⚠️ Endpoint 中的业务逻辑（方法体超过 10 行）

### 需要人工判定（Level 3）
- ⚠️ 同步跨模块命令调用
- ⚠️ Handler 中的复杂业务逻辑（应在领域模型中）

**执行级别详情**：[ADR-0005-Enforcement-Levels.md](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

---

## 参考

详细场景和示例：
- [adr-0001.prompts.md](../../docs/copilot/adr-0001.prompts.md) - 模块隔离场景指导
- [adr-0005.prompts.md](../../docs/copilot/adr-0005.prompts.md) - Handler 模式和 CQRS 场景指导
- [architecture-test-failures.md](../../docs/copilot/architecture-test-failures.md) - 测试失败诊断

---

## 维护提醒

> **🔄 重要**  
> 如本文件内容与 ADR 正文存在不一致，或架构演进导致规则变更，请：
> 1. 同步架构负责人确认变更
> 2. 更新本文件以与 ADR 正文保持一致
> 3. 进行团队公告，确保所有成员知晓变更
> 4. 更新相关的 [`docs/copilot/`](../../docs/copilot/) 辅导材料
> 5. 确保架构测试与 ADR 正文保持同步

---
