# 案例：Handler 单元测试

> ⚠️ **无裁决力声明**：本文档为实践案例说明，不具备架构裁决权。所有架构决策以 [ADR 文档](../adr/) 为准。

**难度**：🟢 简单  
**相关 ADR**：[ADR-0005](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md), [ADR-0905](../adr/governance/ADR-905-testing-architecture-final.md)  
**作者**：@douhuaa  
**日期**：2026-01-27  
**标签**：测试, Handler, CQRS, 单元测试, Mocking

---

## 适用场景

为 Command Handler 和 Query Handler 编写独立的单元测试，验证：
- 业务编排逻辑正确
- 依赖调用符合预期
- 异常处理符合规范
- 返回值符合约定

**测试目标**：
- Command Handler：验证编排流程和副作用
- Query Handler：验证数据查询和组合逻辑

---

## 背景

根据 ADR-0005 和 ADR-0905，Handler 是用例的唯一入口点，负责编排业务流程。Handler 的单元测试应该：

1. **隔离测试**：使用 mock 替换外部依赖
2. **快速反馈**：不依赖数据库或外部服务
3. **明确意图**：一个测试验证一个行为
4. **遵循 AAA**：Arrange-Act-Assert 模式

---

## 解决方案

### 架构设计

```
测试金字塔
┌────────────────────┐
│   E2E Tests        │  少量，覆盖关键路径
├────────────────────┤
│ Integration Tests  │  中等，验证模块交互
├────────────────────┤
│   Unit Tests       │  大量，快速验证逻辑  ← 本案例聚焦
└────────────────────┘
```

**Handler 单元测试特点**：
- 使用 mock 框架（如 NSubstitute）
- 不启动数据库或外部服务
- 执行速度快（毫秒级）
- 可并行运行

---

### 代码实现

#### 前置条件：测试项目结构

```
src/tests/
├── Modules.Orders.Tests/          ← 模块测试项目
│   ├── UseCases/
│   │   ├── CreateOrder/
│   │   │   └── CreateOrderHandlerTests.cs
│   │   └── GetOrderDetails/
│   │       └── GetOrderDetailsHandlerTests.cs
│   └── Domain/
│       └── OrderTests.cs
├── ArchitectureTests/              ← 架构测试
└── IntegrationTests/               ← 集成测试
```

**遵循原则**（根据 ADR-0905）：
- 测试项目镜像源代码结构
- 每个 Handler 对应一个测试类

---

### 案例 1：Command Handler 单元测试

#### 被测试的 Handler

```csharp
// src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs
namespace Zss.BilliardHall.Modules.Orders.UseCases.CreateOrder;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Guid>
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository repository,
        IEventBus eventBus,
        ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateOrder command)
    {
        _logger.LogInformation(
            "Creating order for member {MemberId}",
            command.MemberId);

        // 1. 创建领域对象
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

        _logger.LogInformation(
            "Order {OrderId} created successfully",
            order.Id);

        // 4. 返回 ID（根据 ADR-0005）
        return order.Id;
    }
}
```

#### 单元测试实现

```csharp
// src/tests/Modules.Orders.Tests/UseCases/CreateOrder/CreateOrderHandlerTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Zss.BilliardHall.Modules.Orders.Tests.UseCases.CreateOrder;

public class CreateOrderHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        // Arrange - 创建 mock 依赖
        _repository = Substitute.For<IOrderRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<CreateOrderHandler>>();
        
        _handler = new CreateOrderHandler(
            _repository,
            _eventBus,
            _logger
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrderAndReturnsId()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new CreateOrder(
            MemberId: memberId,
            Items: new[]
            {
                new OrderItem("product1", Quantity: 2, Price: 100m),
                new OrderItem("product2", Quantity: 1, Price: 50m)
            }
        );

        // Act
        var orderId = await _handler.Handle(command);

        // Assert
        orderId.Should().NotBeEmpty();
        
        // 验证保存被调用
        await _repository.Received(1).SaveAsync(
            Arg.Is<Order>(o => 
                o.MemberId == memberId &&
                o.Items.Count == 2
            )
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesOrderCreatedEvent()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new CreateOrder(
            MemberId: memberId,
            Items: new[] { new OrderItem("product1", 1, 100m) }
        );

        // Act
        var orderId = await _handler.Handle(command);

        // Assert
        await _eventBus.Received(1).PublishAsync(
            Arg.Is<OrderCreatedEvent>(e =>
                e.OrderId == orderId &&
                e.MemberId == memberId &&
                e.TotalAmount == 100m
            )
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_LogsInformation()
    {
        // Arrange
        var command = new CreateOrder(
            MemberId: Guid.NewGuid(),
            Items: new[] { new OrderItem("product1", 1, 100m) }
        );

        // Act
        await _handler.Handle(command);

        // Assert - 验证日志调用
        _logger.Received().LogInformation(
            Arg.Is<string>(msg => msg.Contains("Creating order")),
            Arg.Any<Guid>()
        );
        
        _logger.Received().LogInformation(
            Arg.Is<string>(msg => msg.Contains("created successfully")),
            Arg.Any<Guid>()
        );
    }

    [Fact]
    public async Task Handle_RepositoryThrows_PropagatesException()
    {
        // Arrange
        var command = new CreateOrder(
            MemberId: Guid.NewGuid(),
            Items: new[] { new OrderItem("product1", 1, 100m) }
        );

        _repository.SaveAsync(Arg.Any<Order>())
            .Returns(Task.FromException(new DatabaseException("Connection failed")));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command))
            .Should().ThrowAsync<DatabaseException>()
            .WithMessage("Connection failed");
        
        // 验证事件未发布
        await _eventBus.DidNotReceive().PublishAsync(Arg.Any<OrderCreatedEvent>());
    }

    [Fact]
    public async Task Handle_EmptyItems_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateOrder(
            MemberId: Guid.NewGuid(),
            Items: Array.Empty<OrderItem>()  // 空的商品列表
        );

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one item*");
    }
}
```

**测试要点**：
- ✅ 每个测试方法验证一个行为
- ✅ 使用 `FluentAssertions` 提高可读性
- ✅ 使用 `NSubstitute.Received()` 验证交互
- ✅ 测试成功路径和异常路径
- ✅ 清晰的测试命名：`方法名_场景_预期结果`

---

### 案例 2：Query Handler 单元测试

#### 被测试的 Handler

```csharp
// src/Modules/Orders/UseCases/GetOrderDetails/GetOrderDetailsHandler.cs
namespace Zss.BilliardHall.Modules.Orders.UseCases.GetOrderDetails;

public sealed class GetOrderDetailsHandler 
    : IQueryHandler<GetOrderDetails, OrderDetailsDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberQueryService _memberQueryService;

    public GetOrderDetailsHandler(
        IOrderRepository orderRepository,
        IMemberQueryService memberQueryService)
    {
        _orderRepository = orderRepository;
        _memberQueryService = memberQueryService;
    }

    public async Task<OrderDetailsDto> Handle(GetOrderDetails query)
    {
        // 1. 查询订单
        var order = await _orderRepository.GetByIdAsync(query.OrderId);
        
        if (order == null)
        {
            throw new OrderNotFoundException(query.OrderId);
        }

        // 2. 跨模块查询会员信息
        var memberInfo = await _memberQueryService
            .GetMemberInfoAsync(order.MemberId);

        // 3. 组合返回 DTO
        return new OrderDetailsDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            MemberInfo = memberInfo != null ? new MemberInfoDto
            {
                MemberId = memberInfo.MemberId,
                Name = memberInfo.Name,
                Email = memberInfo.Email
            } : null
        };
    }
}
```

#### 单元测试实现

```csharp
// src/tests/Modules.Orders.Tests/UseCases/GetOrderDetails/GetOrderDetailsHandlerTests.cs
public class GetOrderDetailsHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberQueryService _memberQueryService;
    private readonly GetOrderDetailsHandler _handler;

    public GetOrderDetailsHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _memberQueryService = Substitute.For<IMemberQueryService>();
        
        _handler = new GetOrderDetailsHandler(
            _orderRepository,
            _memberQueryService
        );
    }

    [Fact]
    public async Task Handle_OrderExists_ReturnsDetailsWithMemberInfo()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        
        var order = new Order(memberId, new[]
        {
            new OrderItem("product1", 2, 100m)
        });
        _orderRepository.GetByIdAsync(orderId).Returns(order);
        
        var memberInfo = new MemberInfoContract
        {
            MemberId = memberId,
            Name = "张三",
            Email = "zhang@example.com"
        };
        _memberQueryService.GetMemberInfoAsync(memberId).Returns(memberInfo);
        
        var query = new GetOrderDetails(orderId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.TotalAmount.Should().Be(200m);
        result.MemberInfo.Should().NotBeNull();
        result.MemberInfo!.Name.Should().Be("张三");
        result.MemberInfo.Email.Should().Be("zhang@example.com");
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId).Returns((Order?)null);
        
        var query = new GetOrderDetails(orderId);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(query))
            .Should().ThrowAsync<OrderNotFoundException>()
            .Where(ex => ex.OrderId == orderId);
        
        // 验证未查询会员信息
        await _memberQueryService.DidNotReceive()
            .GetMemberInfoAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_MemberNotFound_ReturnsOrderWithNullMemberInfo()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        
        var order = new Order(memberId, new[] 
        { 
            new OrderItem("product1", 1, 50m) 
        });
        _orderRepository.GetByIdAsync(orderId).Returns(order);
        
        _memberQueryService.GetMemberInfoAsync(memberId)
            .Returns((MemberInfoContract?)null);
        
        var query = new GetOrderDetails(orderId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.MemberInfo.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MemberServiceThrows_PropagatesException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        
        var order = new Order(memberId, new[] 
        { 
            new OrderItem("product1", 1, 50m) 
        });
        _orderRepository.GetByIdAsync(orderId).Returns(order);
        
        _memberQueryService.GetMemberInfoAsync(memberId)
            .Returns(Task.FromException<MemberInfoContract?>(
                new ServiceUnavailableException("Member service down")
            ));
        
        var query = new GetOrderDetails(orderId);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(query))
            .Should().ThrowAsync<ServiceUnavailableException>();
    }
}
```

---

### 测试组织与命名

#### 测试类命名

```csharp
// ✅ 好的命名
public class CreateOrderHandlerTests { }
public class GetOrderDetailsHandlerTests { }
public class CancelOrderHandlerTests { }

// ❌ 不好的命名
public class OrderTests { }          // 过于宽泛
public class CreateOrderTest { }     // 缺少复数
public class TestCreateOrder { }     // 不符合约定
```

#### 测试方法命名

使用模式：`方法名_场景_预期结果`

```csharp
// ✅ 好的命名
[Fact]
public async Task Handle_ValidCommand_CreatesOrderAndReturnsId()

[Fact]
public async Task Handle_OrderNotFound_ThrowsOrderNotFoundException()

[Fact]
public async Task Handle_EmptyItems_ThrowsValidationException()

// ❌ 不好的命名
[Fact]
public async Task Test1()  // 不描述行为

[Fact]
public async Task CreateOrder()  // 不明确测试什么

[Fact]
public async Task ShouldWork()  // 过于模糊
```

---

## 常见陷阱

### ❌ 陷阱 1：测试实现细节而非行为

```csharp
// ❌ 错误：过度依赖实现细节
[Fact]
public async Task Handle_ValidCommand_CallsRepositorySaveAsync()
{
    // Act
    await _handler.Handle(command);
    
    // Assert - 只验证方法被调用，不验证行为
    await _repository.Received(1).SaveAsync(Arg.Any<Order>());
}
```

**问题**：
- 测试关注"如何做"而非"做了什么"
- 重构实现会导致测试失败

**正确做法**：
```csharp
// ✅ 正确：验证行为和结果
[Fact]
public async Task Handle_ValidCommand_CreatesOrderWithCorrectData()
{
    // Act
    var orderId = await _handler.Handle(command);
    
    // Assert - 验证行为（返回了有效ID）和副作用（正确保存）
    orderId.Should().NotBeEmpty();
    await _repository.Received(1).SaveAsync(
        Arg.Is<Order>(o => 
            o.MemberId == command.MemberId &&
            o.Items.Count == command.Items.Length
        )
    );
}
```

### ❌ 陷阱 2：一个测试验证多个行为

```csharp
// ❌ 错误：一个测试做太多事情
[Fact]
public async Task Handle_MultipleScenarios()
{
    // 场景 1：成功创建
    var result1 = await _handler.Handle(validCommand);
    result1.Should().NotBeEmpty();
    
    // 场景 2：空商品列表
    await _handler.Invoking(h => h.Handle(emptyCommand))
        .Should().ThrowAsync<ValidationException>();
    
    // 场景 3：仓储失败
    _repository.SaveAsync(Arg.Any<Order>())
        .Returns(Task.FromException(new Exception()));
    await _handler.Invoking(h => h.Handle(validCommand))
        .Should().ThrowAsync<Exception>();
}
```

**问题**：
- 测试失败时不清楚哪个场景出错
- 违反"单一职责"原则
- 难以维护

**正确做法**：
```csharp
// ✅ 正确：每个测试一个场景
[Fact]
public async Task Handle_ValidCommand_CreatesOrder() { }

[Fact]
public async Task Handle_EmptyItems_ThrowsValidationException() { }

[Fact]
public async Task Handle_RepositoryFails_PropagatesException() { }
```

### ❌ 陷阱 3：过度 mocking

```csharp
// ❌ 错误：mock 了不应该 mock 的东西
[Fact]
public async Task Handle_ValidCommand_CreatesOrder()
{
    // ❌ Mock 领域对象
    var order = Substitute.For<Order>();
    order.Id.Returns(Guid.NewGuid());
    order.TotalAmount.Returns(100m);
    
    // ❌ Mock 值对象
    var items = Substitute.For<List<OrderItem>>();
    
    // ...
}
```

**问题**：
- 领域对象应该真实创建，不应该 mock
- 破坏了单元测试的价值

**正确做法**：
```csharp
// ✅ 正确：只 mock 依赖和接口
[Fact]
public async Task Handle_ValidCommand_CreatesOrder()
{
    // ✅ 真实的领域对象
    var command = new CreateOrder(
        MemberId: Guid.NewGuid(),
        Items: new[] { new OrderItem("product1", 1, 100m) }
    );
    
    // ✅ Mock 外部依赖
    _repository = Substitute.For<IOrderRepository>();
    _eventBus = Substitute.For<IEventBus>();
    
    // Act
    var orderId = await _handler.Handle(command);
    
    // Assert
    orderId.Should().NotBeEmpty();
}
```

---

## 最佳实践

### ✅ 实践 1：使用测试数据构建器

对于复杂的测试数据，使用构建器模式：

```csharp
public class CreateOrderCommandBuilder
{
    private Guid _memberId = Guid.NewGuid();
    private List<OrderItem> _items = new() 
    { 
        new("product1", 1, 100m) 
    };

    public CreateOrderCommandBuilder WithMember(Guid memberId)
    {
        _memberId = memberId;
        return this;
    }

    public CreateOrderCommandBuilder WithItems(params OrderItem[] items)
    {
        _items = items.ToList();
        return this;
    }

    public CreateOrder Build() => new(_memberId, _items.ToArray());
}

// 使用
var command = new CreateOrderCommandBuilder()
    .WithMember(memberId)
    .WithItems(
        new OrderItem("product1", 2, 100m),
        new OrderItem("product2", 1, 50m)
    )
    .Build();
```

### ✅ 实践 2：参数化测试

使用 `[Theory]` 测试多个输入：

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public async Task Handle_InvalidQuantity_ThrowsValidationException(int quantity)
{
    // Arrange
    var command = new CreateOrder(
        MemberId: Guid.NewGuid(),
        Items: new[] { new OrderItem("product1", quantity, 100m) }
    );

    // Act & Assert
    await _handler.Invoking(h => h.Handle(command))
        .Should().ThrowAsync<ValidationException>()
        .WithMessage("*quantity must be positive*");
}
```

### ✅ 实践 3：共享测试设置

使用构造函数和 `IClassFixture` 共享设置：

```csharp
public class CreateOrderHandlerTests : IDisposable
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        // 每个测试方法前执行
        _repository = Substitute.For<IOrderRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _handler = new CreateOrderHandler(_repository, _eventBus);
    }

    public void Dispose()
    {
        // 每个测试方法后执行（如需要）
    }

    // 测试方法...
}
```

---

## 架构合规检查清单

根据 ADR-0905，确认：

- [ ] 测试项目镜像源代码结构
- [ ] 每个 Handler 有对应的测试类
- [ ] 测试命名清晰（`方法名_场景_预期结果`）
- [ ] 使用 mock 隔离外部依赖
- [ ] 测试覆盖成功路径和异常路径
- [ ] 每个测试只验证一个行为
- [ ] 使用 FluentAssertions 提高可读性
- [ ] 测试运行快速（不依赖数据库）

---

## 参考资料

- [ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md) - Handler 规则
- [ADR-0905：三层测试架构：单元、集成、架构](../adr/governance/ADR-905-testing-architecture-final.md) - 测试架构
- [测试架构指南](../guides/test-architecture-guide.md)
- [测试框架指南](../guides/testing-framework-guide.md)

---

**维护**：Tech Lead  
**状态**：✅ Active  
**最后更新**：2026-01-27
