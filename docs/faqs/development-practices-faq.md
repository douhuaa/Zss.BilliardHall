# 开发实践常见问题

> 📚 **根据 ADR-950 创建的 FAQ 文档**  
> **对应 ADR**：ADR-0005, ADR-0000, ADR-0930  
> **最后更新**：2026-01-27

---

## 概述

本文档解答关于日常开发实践、代码规范、测试编写的常见问题。

---

## 代码组织

### Q: 如何为新功能创建文件结构？

**A**: 

遵循垂直切片组织，每个用例自包含所有必要文件。

**示例**：为 Orders 模块添加"取消订单"功能

```
src/Modules/Orders/UseCases/CancelOrder/
├─ CancelOrder.cs                    // Command 定义
├─ CancelOrderHandler.cs             // Handler 实现
├─ CancelOrderEndpoint.cs            // API 端点
└─ CancelOrderValidator.cs           // 可选：输入验证

src/tests/Modules.Orders.Tests/UseCases/CancelOrder/
└─ CancelOrderHandlerTests.cs        // 单元测试
```

**步骤**：
1. 在 `UseCases/` 下创建功能目录（如 `CancelOrder/`）
2. 创建 Command 或 Query 类
3. 创建对应的 Handler
4. 创建 Endpoint（如果需要 API）
5. 在测试项目中镜像相同结构
6. 编写单元测试

**参考 ADR**：[ADR-0001](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 3.2 节

---

### Q: 代码文件应该有多大？什么时候需要拆分？

**A**: 

**经验法则**：
- Handler 类：通常 50-150 行
- 领域对象：取决于业务复杂度，但单个方法不超过 30 行
- Endpoint：通常 20-50 行（应该很薄）

**需要拆分的信号**：
- 单个方法超过 30 行
- 类超过 300 行
- 一个类处理多个职责
- 测试变得困难

**如何拆分**：
```csharp
// ❌ 过大的 Handler（混合多个职责）
public class CreateOrderHandler
{
    public async Task<Guid> Handle(CreateOrder command)
    {
        // 100 行验证逻辑
        // 80 行业务规则
        // 50 行数据保存
        // 30 行事件发布
        // 总计 260 行
    }
}

// ✅ 重构后
public class CreateOrderHandler
{
    private readonly IOrderFactory _factory;
    private readonly IOrderRepository _repository;
    
    public async Task<Guid> Handle(CreateOrder command)
    {
        // 委托给工厂创建（封装复杂逻辑）
        var order = _factory.CreateFrom(command);
        
        // 保存
        await _repository.SaveAsync(order);
        
        // 发布事件（在领域对象内）
        foreach (var @event in order.DomainEvents)
        {
            await _eventBus.PublishAsync(@event);
        }
        
        return order.Id;
    }
}
```

---

### Q: 如何命名 Command、Query、Handler？

**A**: 

**命名约定**：

1. **Command**：动词 + 名词
   ```csharp
   CreateOrder           // ✅
   CancelOrder           // ✅
   UpdateMemberProfile   // ✅
   
   OrderCreate           // ❌ 错误的顺序
   DoCreateOrder         // ❌ 多余的 Do
   ```

2. **Query**：Get/List + 名词 + 可选条件
   ```csharp
   GetOrderDetails       // ✅
   ListActiveOrders      // ✅
   GetMembersByLevel     // ✅
   
   OrderDetails          // ❌ 缺少动词
   FetchOrder            // ❌ 使用 Get 更一致
   ```

3. **Handler**：Command/Query 名称 + Handler
   ```csharp
   CreateOrderHandler             // ✅
   GetOrderDetailsHandler         // ✅
   
   OrderCreationHandler           // ❌ 不对应 Command 名称
   HandleCreateOrder              // ❌ 不符合约定
   ```

4. **Endpoint**：Command/Query 名称 + Endpoint
   ```csharp
   CreateOrderEndpoint            // ✅
   GetOrderDetailsEndpoint        // ✅
   ```

**参考 ADR**：[ADR-0930](../adr/governance/ADR-0930-code-style-governance.md)

---

## 测试编写

### Q: 测试应该放在哪里？如何命名测试类和方法？

**A**: 

**测试项目结构**（必须镜像源代码）：

```
// 源代码
src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs

// 测试代码
src/tests/Modules.Orders.Tests/UseCases/CreateOrder/CreateOrderHandlerTests.cs
```

**测试类命名**：
```csharp
// ✅ 正确
public class CreateOrderHandlerTests { }
public class OrderTests { }                // 测试 Order 领域对象

// ❌ 错误
public class CreateOrderTest { }           // 缺少复数
public class TestCreateOrderHandler { }    // 不符合约定
```

**测试方法命名**：使用模式 `方法名_场景_预期结果`

```csharp
// ✅ 正确
[Fact]
public async Task Handle_ValidCommand_CreatesOrderAndReturnsId()

[Fact]
public async Task Handle_EmptyItems_ThrowsValidationException()

[Fact]
public async Task Handle_InactiveMember_ThrowsBusinessException()

// ❌ 错误
[Fact]
public async Task Test1()                   // 不描述行为

[Fact]
public async Task CreateOrder()             // 不明确测试什么

[Fact]
public async Task ShouldCreateOrder()       // 冗余的 Should
```

**参考**：
- [ADR-0000：架构测试与 CI 治理元规则](../adr/governance/ADR-0000-architecture-tests.md)
- [Handler 单元测试案例](../cases/handler-unit-testing-case.md)

---

### Q: 应该 mock 什么，不应该 mock 什么？

**A**: 

**应该 mock 的**（外部依赖和基础设施）：
- ✅ 仓储接口（`IOrderRepository`）
- ✅ 外部服务（`IPaymentGateway`, `IEmailService`）
- ✅ 事件总线（`IEventBus`）
- ✅ 时间提供者（`IDateTimeProvider`）
- ✅ 跨模块查询服务（`IMemberQueryService`）

**不应该 mock 的**（值对象和领域模型）：
- ❌ 值对象（`Money`, `Email`, `Address`）
- ❌ 领域对象（`Order`, `Member`）
- ❌ Command/Query 对象
- ❌ DTO/Contract 对象
- ❌ 简单的数据结构

**示例**：

```csharp
// ✅ 正确的单元测试
[Fact]
public async Task Handle_ValidCommand_CreatesOrder()
{
    // Mock 外部依赖
    var repository = Substitute.For<IOrderRepository>();
    var eventBus = Substitute.For<IEventBus>();
    
    var handler = new CreateOrderHandler(repository, eventBus);
    
    // 真实的 Command 和领域对象
    var command = new CreateOrder(
        MemberId: Guid.NewGuid(),
        Items: new[] { new OrderItem("product1", 1, 100m) }
    );
    
    // Act
    var orderId = await handler.Handle(command);
    
    // Assert
    orderId.Should().NotBeEmpty();
    await repository.Received(1).SaveAsync(Arg.Any<Order>());
}

// ❌ 错误：mock 了不该 mock 的东西
[Fact]
public async Task Handle_MockedDomainObjects()
{
    var repository = Substitute.For<IOrderRepository>();
    var eventBus = Substitute.For<IEventBus>();
    
    // ❌ 不要 mock 领域对象
    var order = Substitute.For<Order>();
    order.Id.Returns(Guid.NewGuid());
    
    // ❌ 不要 mock Command
    var command = Substitute.For<CreateOrder>();
    
    // 这样的测试没有价值
}
```

**参考**：[Handler 单元测试案例](../cases/handler-unit-testing-case.md)

---

### Q: 单元测试和集成测试的区别？分别测试什么？

**A**: 

**单元测试**（Unit Tests）：
- **范围**：单个类（通常是 Handler 或领域对象）
- **依赖**：使用 mock 隔离
- **速度**：非常快（毫秒级）
- **目的**：验证逻辑正确性

```csharp
// 单元测试示例
public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesOrder()
    {
        // Mock 所有依赖
        var repository = Substitute.For<IOrderRepository>();
        var eventBus = Substitute.For<IEventBus>();
        
        var handler = new CreateOrderHandler(repository, eventBus);
        
        // 测试 Handler 逻辑
        var orderId = await handler.Handle(command);
        
        orderId.Should().NotBeEmpty();
    }
}
```

**集成测试**（Integration Tests）：
- **范围**：多个组件协作（Handler + 数据库 + 事件总线）
- **依赖**：使用真实依赖（测试数据库）
- **速度**：较慢（秒级）
- **目的**：验证组件集成正确

```csharp
// 集成测试示例
[Collection("Integration")]
public class CreateOrderIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    [Fact]
    public async Task CreateOrder_EndToEnd_Success()
    {
        // 使用真实的数据库和事件总线
        var command = new CreateOrder(...);
        
        var orderId = await _fixture.SendAsync(command);
        
        // 验证数据库中确实创建了订单
        var order = await _fixture.FindAsync<Order>(orderId);
        order.Should().NotBeNull();
        
        // 验证事件确实被发布
        var events = _fixture.PublishedEvents<OrderCreatedEvent>();
        events.Should().ContainSingle();
    }
}
```

**什么时候用哪种**：
- 业务逻辑验证 → 单元测试（快速反馈）
- 数据库查询和映射 → 集成测试
- 事件流程端到端 → 集成测试
- 边界情况和异常 → 单元测试

**参考 ADR**：[ADR-0000：架构测试与 CI 治理元规则](../adr/governance/ADR-0000-architecture-tests.md)

---

## 依赖注入

### Q: 如何注册和使用依赖？

**A**: 

**模块内注册**（在模块的扩展方法中）：

```csharp
// src/Modules/Orders/OrdersModule.cs
public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services)
    {
        // 1. 注册仓储
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // 2. 注册所有 Handler（自动扫描）
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(OrdersModule).Assembly));
        
        // 3. 注册领域服务（如果有）
        services.AddScoped<IOrderDomainService, OrderDomainService>();
        
        return services;
    }
}
```

**在 Handler 中使用**：

```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrder, Guid>
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;
    
    // 构造函数注入
    public CreateOrderHandler(
        IOrderRepository repository,
        IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }
    
    public async Task<Guid> Handle(CreateOrder command)
    {
        // 使用注入的依赖
        var order = new Order(...);
        await _repository.SaveAsync(order);
        return order.Id;
    }
}
```

**生命周期选择**：
- `AddTransient` - 每次请求创建新实例（无状态服务）
- `AddScoped` - 每个 HTTP 请求一个实例（仓储、Handler）
- `AddSingleton` - 应用启动时创建，全局共享（配置、缓存）

**参考**：[架构设计指南](../guides/architecture-design-guide.md)

---

### Q: 可以在 Handler 中直接注入 DbContext 吗？

**A**: 

**不推荐。应该通过仓储接口访问数据。**

**原因**：
1. **抽象隔离**：Handler 不应该知道数据存储的细节
2. **可测试性**：仓储接口易于 mock，DbContext 难以测试
3. **业务语义**：`_repository.SaveAsync(order)` 比 `_dbContext.Orders.Add()` 更清晰
4. **切换存储**：未来可以更换数据库或添加缓存层

```csharp
// ❌ 不推荐：直接注入 DbContext
public class CreateOrderHandler
{
    private readonly AppDbContext _dbContext;
    
    public async Task<Guid> Handle(CreateOrder command)
    {
        var order = new Order(...);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return order.Id;
    }
}

// ✅ 推荐：通过仓储接口
public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    
    public async Task<Guid> Handle(CreateOrder command)
    {
        var order = new Order(...);
        await _repository.SaveAsync(order);
        return order.Id;
    }
}
```

**例外情况**：
- 复杂的只读查询（Query Handler）可以直接使用 DbContext
- 需要特殊的查询优化时

---

## 异常处理

### Q: 应该在哪里捕获异常？如何返回错误信息？

**A**: 

**异常处理层级**：

1. **领域对象**：抛出领域异常
   ```csharp
   public class Order
   {
       public void ApplyDiscount(decimal percentage)
       {
           if (percentage < 0 || percentage > 100)
           {
               throw new InvalidDiscountException(
                   $"Discount {percentage} is invalid");
           }
           // ...
       }
   }
   ```

2. **Handler**：让异常传播（通常不捕获）
   ```csharp
   public class CreateOrderHandler
   {
       public async Task<Guid> Handle(CreateOrder command)
       {
           // 不捕获领域异常，让它传播到上层
           var order = new Order(...);
           order.ApplyDiscount(command.Discount);  // 可能抛出异常
           
           await _repository.SaveAsync(order);
           return order.Id;
       }
   }
   ```

3. **全局异常处理器**：统一转换为 HTTP 响应
   ```csharp
   // src/Host/Middleware/GlobalExceptionHandler.cs
   public class GlobalExceptionHandler : IExceptionHandler
   {
       public async ValueTask<bool> TryHandleAsync(
           HttpContext context,
           Exception exception,
           CancellationToken cancellationToken)
       {
           var (statusCode, title) = exception switch
           {
               ValidationException => (400, "Validation Error"),
               NotFoundException => (404, "Not Found"),
               BusinessException => (422, "Business Rule Violation"),
               _ => (500, "Internal Server Error")
           };
           
           var problemDetails = new ProblemDetails
           {
               Status = statusCode,
               Title = title,
               Detail = exception.Message
           };
           
           await context.Response.WriteAsJsonAsync(
               problemDetails,
               cancellationToken);
           
           return true;
       }
   }
   ```

**不要**：
- ❌ 在 Handler 中捕获并返回错误对象
- ❌ 吞掉异常（catch 但不重新抛出）
- ❌ 使用异常控制正常业务流程

**参考**：[Handler 异常处理标准](../guides/handler-exception-retry-standard.md)

---

## 日志记录

### Q: 应该记录什么日志？如何组织日志信息？

**A**: 

**日志级别使用**：

- **Information**：关键业务流程节点
  ```csharp
  _logger.LogInformation(
      "Order {OrderId} created for member {MemberId}",
      order.Id, 
      command.MemberId);
  ```

- **Warning**：可恢复的异常或异常情况
  ```csharp
  _logger.LogWarning(
      "Member {MemberId} not found when creating order, using default profile",
      command.MemberId);
  ```

- **Error**：业务异常或错误
  ```csharp
  _logger.LogError(
      exception,
      "Failed to save order {OrderId}",
      order.Id);
  ```

- **Debug**：详细的调试信息（生产环境关闭）
  ```csharp
  _logger.LogDebug(
      "Validating order items: {ItemCount} items",
      command.Items.Length);
  ```

**最佳实践**：

1. **使用结构化日志**（不要拼接字符串）
   ```csharp
   // ✅ 正确
   _logger.LogInformation(
       "Order {OrderId} total is {Amount}",
       orderId,
       amount);
   
   // ❌ 错误
   _logger.LogInformation(
       $"Order {orderId} total is {amount}");
   ```

2. **记录关键业务事件**
   - 用例开始和完成
   - 重要的业务决策点
   - 外部服务调用
   - 领域事件发布

3. **包含关联 ID**（用于追踪请求链）
   ```csharp
   _logger.LogInformation(
       "Processing order {OrderId} for request {RequestId}",
       orderId,
       httpContext.TraceIdentifier);
   ```

4. **不要记录敏感信息**
   - ❌ 密码、令牌
   - ❌ 信用卡号
   - ❌ 个人身份信息（除非脱敏）

**参考**：[结构化日志与监控标准](../guides/structured-logging-monitoring-standard.md)

---

## 性能优化

### Q: 如何避免 N+1 查询问题？

**A**: 

**问题示例**：
```csharp
// ❌ N+1 查询问题
public async Task<List<OrderDto>> Handle(ListOrders query)
{
    var orders = await _repository.GetAllAsync();
    
    var result = new List<OrderDto>();
    foreach (var order in orders)
    {
        // 每个订单都查询一次会员信息 - N 次查询
        var memberInfo = await _memberService
            .GetMemberInfoAsync(order.MemberId);
        
        result.Add(new OrderDto 
        { 
            OrderId = order.Id,
            MemberName = memberInfo?.Name 
        });
    }
    
    return result;  // 1 + N 次查询
}
```

**解决方案**：

**方案 1：批量查询**
```csharp
// ✅ 使用批量查询
public async Task<List<OrderDto>> Handle(ListOrders query)
{
    var orders = await _repository.GetAllAsync();
    
    // 收集所有会员 ID
    var memberIds = orders.Select(o => o.MemberId).Distinct();
    
    // 一次性批量查询所有会员
    var members = await _memberService.GetMembersInfoAsync(memberIds);
    var memberDict = members.ToDictionary(m => m.MemberId);
    
    // 组合结果
    return orders.Select(order => new OrderDto
    {
        OrderId = order.Id,
        MemberName = memberDict.TryGetValue(order.MemberId, out var member)
            ? member.Name
            : "Unknown"
    }).ToList();
}
```

**方案 2：使用 Include（Entity Framework）**
```csharp
// ✅ 使用 EF Core Include 预加载关联数据
public async Task<List<Order>> GetOrdersWithMembersAsync()
{
    return await _dbContext.Orders
        .Include(o => o.Member)  // 使用 JOIN 一次性加载
        .ToListAsync();
}
```

**方案 3：投影查询（最优）**
```csharp
// ✅ 直接查询需要的字段
public async Task<List<OrderDto>> Handle(ListOrders query)
{
    return await _dbContext.Orders
        .Select(o => new OrderDto
        {
            OrderId = o.Id,
            TotalAmount = o.TotalAmount,
            MemberName = o.Member.Name  // EF Core 会生成高效的 JOIN
        })
        .ToListAsync();
}
```

---

## 相关文档

- [ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0000：架构测试与 CI 治理元规则](../adr/governance/ADR-0000-architecture-tests.md)
- [ADR-0930：代码风格治理](../adr/governance/ADR-0930-code-style-governance.md)
- [架构设计指南](../guides/architecture-design-guide.md)
- [Handler 单元测试案例](../cases/handler-unit-testing-case.md)

---

**维护**：Tech Lead  
**最后审核**：2026-01-27  
**状态**：✅ Active
