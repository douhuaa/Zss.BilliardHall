# ADR-006: InvokeAsync 仅限进程内使用的限制

## 状态

**接受**

## 上下文

Wolverine 的 `IMessageBus.InvokeAsync()` 提供了同步调用其他 Handler 的能力，但如果在跨服务场景误用，会导致紧耦合和性能问题。

### 面临的问题

1. **InvokeAsync 的诱惑**：
   - 语法简单，看起来像本地方法调用
   - 开发者容易忽视跨服务边界
   - 不小心用于远程调用，导致同步等待外部服务

2. **跨服务同步调用的风险**：
   - 紧耦合：调用方依赖外部服务可用性
   - 性能下降：同步等待增加延迟
   - 级联失败：外部服务故障导致本服务不可用
   - 超时难以控制：网络延迟不可预测

3. **现实案例**：
   ```csharp
   // ❌ 错误：尝试同步调用外部支付服务
   public class OrderHandler
   {
       public async Task Handle(CreateOrder cmd, IMessageBus bus)
       {
           // 错误！外部支付服务不应该用 InvokeAsync
           var result = await bus.InvokeAsync<PaymentResult>(
               new ProcessExternalPayment(...)  // 外部服务！
           );
           // 如果支付服务挂了，订单服务也挂了
       }
   }
   ```

## 决策

**`InvokeAsync()` 只能用于进程内模块通信，禁止跨服务使用。**

### 核心原则

> **铁律**: `InvokeAsync` 只能调用同一应用进程内的 Handler

### ✅ 允许（进程内）

**同一应用内的不同模块**:
```csharp
// ✅ Billing 模块在同一进程
public class EndSessionHandler
{
    public async Task Handle(EndSession command, IMessageBus bus)
    {
        // 同步调用同一进程内的 Billing 模块
        var bill = await bus.InvokeAsync<BillResult>(
            new CalculateBill(sessionId, duration)
        );
        
        // 继续处理...
    }
}
```

**使用场景**:
- 跨模块同步查询（如获取会员余额）
- 跨模块业务规则校验（如检查库存）
- 需要立即得到结果的命令（如生成账单）

### ❌ 禁止（跨服务）

**错误示例 1：同步调用外部支付服务**
```csharp
// ❌ 紧耦合 + 性能风险
public class OrderHandler
{
    public async Task Handle(CreateOrder cmd, IMessageBus bus)
    {
        // 错误：尝试同步调用外部支付服务
        var result = await bus.InvokeAsync<PaymentResult>(
            new ProcessExternalPayment(...)  // 外部服务！
        );
    }
}
```

**错误示例 2：同步调用其他微服务**
```csharp
// ❌ 级联失败风险
public class InventoryHandler
{
    public async Task Handle(ReserveStock cmd, IMessageBus bus)
    {
        // 错误：同步调用外部库存服务
        var available = await bus.InvokeAsync<bool>(
            new CheckStockInWarehouse(...)  // 另一个服务！
        );
    }
}
```

### 跨服务通信的正确方式

#### 方式 1：事件驱动（推荐）

```csharp
// ✅ 发布事件，外部服务订阅
public class OrderHandler
{
    public async Task Handle(CreateOrder cmd, IMessageBus bus)
    {
        // 创建订单
        var order = new Order { /* ... */ };
        
        // 发布事件（异步）
        await bus.PublishAsync(new OrderCreatedIntegrationEvent(order.Id));
        
        // 不等待外部服务响应
        return Result.Ok(order.Id);
    }
}

// 外部支付服务监听事件
public class PaymentServiceListener
{
    public async Task Handle(OrderCreatedIntegrationEvent evt)
    {
        // 异步处理支付
    }
}
```

#### 方式 2：Saga 补偿模式

```csharp
// ✅ 使用 Saga 编排跨服务流程
public class OrderSaga : Saga
{
    public async Task Handle(OrderCreated evt)
    {
        // 发起外部支付请求（异步）
        await PublishAsync(new RequestPayment(evt.OrderId));
    }
    
    // 等待外部服务回调
    public void Handle(PaymentCompleted evt)
    {
        // 继续流程
    }
    
    // 超时补偿
    public void Handle(PaymentTimeout evt)
    {
        // 取消订单
    }
}
```

#### 方式 3：HTTP API（显式远程调用）

```csharp
// ✅ 如果必须同步调用，使用明确的 HTTP 客户端
public class OrderHandler
{
    private readonly IPaymentServiceClient _paymentClient;
    
    public async Task Handle(CreateOrder cmd)
    {
        // 明确这是远程调用（不要伪装成本地消息）
        var result = await _paymentClient.ProcessPaymentAsync(...);
        
        // 添加超时和重试逻辑
    }
}
```

### 防护措施

**配置中明确区分本地和远程**:
```csharp
builder.Host.UseWolverine(opts =>
{
    // ✅ 本地队列 - 允许 InvokeAsync
    opts.LocalQueue("billing").UseDurableInbox();
    opts.LocalQueue("sessions").UseDurableInbox();
    
    // ❌ 外部队列 - 禁止 InvokeAsync（只能 PublishAsync）
    opts.PublishMessage<OrderCreated>()
        .ToRabbitQueue("external-orders")
        .UseDurableOutbox();
    
    // ⚠️ 注意：不要为外部队列配置 Request/Reply
});
```

## 后果

### 正面影响

1. **避免紧耦合**：
   - 服务间松耦合，外部服务故障不影响本服务
   - 降低级联失败风险

2. **性能可控**：
   - 进程内调用性能可预测
   - 避免网络延迟带来的不确定性

3. **架构清晰**：
   - 明确进程内与跨服务边界
   - 强制使用事件驱动或 HTTP

### 负面影响

1. **异步复杂度**：
   - 跨服务必须异步，增加编排复杂度
   - 需要使用 Saga 或事件驱动

2. **开发心理负担**：
   - 需要时刻区分进程内与跨服务
   - 误用 InvokeAsync 需要重构

### 风险缓解

1. **Code Review 强化**：
   - 审查所有 `InvokeAsync` 调用
   - 检查目标 Handler 是否在同一进程

2. **命名约定**：
   - 跨服务消息添加 `IntegrationEvent` 后缀
   - 进程内消息添加 `Command/Query` 后缀

3. **静态分析**：
   - 使用 Roslyn Analyzer 检测跨服务 `InvokeAsync`
   - 编译时警告或错误

## 替代方案

### 方案 A: 允许跨服务同步调用

**描述**: 不限制 `InvokeAsync`，允许跨服务使用

**优点**: 灵活，开发简单

**缺点**:
- 紧耦合，服务不独立
- 性能和可用性风险高
- 级联失败难以控制

**为什么未采纳**: 违背微服务松耦合原则，长期维护灾难。

### 方案 B: 禁止所有 InvokeAsync

**描述**: 完全禁止同步调用，全部用事件

**优点**: 彻底解耦，全异步

**缺点**:
- 进程内也必须异步，过度设计
- 简单查询也要事件，复杂度爆炸
- 失去 Wolverine 的灵活性

**为什么未采纳**: 矫枉过正，进程内同步调用是合理的。

### 方案 C: 自动检测并降级

**描述**: 运行时检测跨服务调用，自动降级为异步

**优点**: 自动化，无需开发者判断

**缺点**:
- 隐藏了问题，开发者不知道被降级
- 行为不可预测
- 调试困难

**为什么未采纳**: 魔法太多，不如显式约束。

## 检查清单

使用 `InvokeAsync` 前，必须回答以下问题：

- [ ] 目标 Handler 是否在同一应用进程内？
- [ ] 是否需要同步获取返回值？
- [ ] 如果是跨服务，为什么不能用事件驱动？
- [ ] 如果必须同步调用外部服务，是否使用 HTTP 客户端？
- [ ] 是否添加了超时和重试逻辑？

## 相关决策

- [ADR-003: 模块间消息通信](./ADR-003-模块间消息通信.md)
- [ADR-007: Saga 使用三条铁律](./ADR-007-Saga使用三条铁律.md)
- [ADR-009: 跨服务通信模式选择](./ADR-009-跨服务通信模式.md)

## 参考资料

- [Request/Reply Pattern - Wolverine Documentation](https://wolverine.netlify.app/guide/messaging/message-bus.html#request-reply)
- [Synchronous vs Asynchronous Microservices](https://www.nginx.com/blog/building-microservices-inter-process-communication/)
- 《Wolverine 模块化架构蓝图》- 6.2 跨进程同步命令的铁律

---

**日期**: 2026-01-19  
**决策者**: 架构团队  
**最后更新**: 2026-01-19
