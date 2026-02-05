# 案例：契约查询模式

> ⚠️ **无裁决力声明**：本文档为实践案例说明，不具备架构裁决权。所有架构决策以 [ADR 文档](../adr/) 为准。

## Metadata

- 难度：🟢 简单
- 级别: Core
- 相关 ADR：[ADR-001](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md), [ADR-005](../adr/constitutional/ADR-005-Application-Interaction-Model-Final.md)
- 作者：@douhuaa
- 日期：2026-01-27
- 标签：模块化, 契约查询, 同步查询, 只读数据, DTO

---

## 适用场景

当一个模块需要**读取**另一个模块的数据用于展示或轻量级决策时，使用契约查询模式。

**典型场景**：
- 订单详情页需要显示会员信息
- 报表需要聚合多个模块的数据
- UI 需要组合来自不同模块的数据

**关键特征**：
- ✅ 同步读取
- ✅ 只读操作
- ✅ 返回 DTO（数据传输对象）
- ❌ 不能用于业务逻辑决策
- ❌ 不能修改数据

---

## 背景

在模块化架构中，模块间不能直接引用领域对象（根据 ADR-001）。但在某些场景下，我们需要跨模块读取数据用于展示。

**为什么不能直接引用领域对象？**
- 创建了编译时依赖
- 暴露了内部实现细节
- 破坏了模块边界

**契约查询的特点**：
- 提供**只读**的数据访问
- 使用**契约（DTO）**，不暴露领域对象
- 查询方不应该用这些数据做业务决策
- 适合展示和报表场景

---

## 解决方案

### 架构设计

```
# 契约查询模式：跨模块同步数据查询
Orders Module                    Members Module
┌────────────────┐              ┌────────────────┐
│ OrderDetails   │              │ Contract Query │
│ Query Handler  │              │ Handler        │
│                │              │                │
│ 1. Query Order │              │ 3. Return      │
│ 2. Query       │─────DTO─────▶│    MemberDTO   │
│    Member Info │              │                │
│ 3. Compose     │              │                │
└────────────────┘              └────────────────┘
```

**关键要素**：
1. **契约定义**：在 BuildingBlocks 中定义 DTO
2. **查询接口**：目标模块提供查询接口
3. **组合查询**：在查询 Handler 中组合数据

---

### 代码示例

#### 步骤 1：定义契约（BuildingBlocks）

```csharp
// src/BuildingBlocks/Zss.BilliardHall.BuildingBlocks/Contracts/Members/MemberInfoContract.cs
namespace Zss.BilliardHall.BuildingBlocks.Contracts.Members;

/// <summary>
/// 会员信息契约
/// 根据 ADR-001，契约是模块间数据共享的合规方式
/// </summary>
public sealed record MemberInfoContract
{
    public Guid MemberId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MembershipLevel { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}
```

**要点**：
- 使用 `record` 类型（不可变）
- 只包含需要展示的数据字段
- **不包含业务逻辑方法**
- 定义在 BuildingBlocks，可被多个模块引用

#### 步骤 2：在 Members 模块中提供查询

##### 2.1 定义查询接口

```csharp
// src/Modules/Members/Contracts/IMemberQueryService.cs
namespace Zss.BilliardHall.Modules.Members.Contracts;

/// <summary>
/// 会员查询服务接口
/// 提供给其他模块的只读数据访问
/// </summary>
public interface IMemberQueryService
{
    /// <summary>
    /// 根据 ID 查询会员信息
    /// </summary>
    Task<MemberInfoContract?> GetMemberInfoAsync(Guid memberId);
    
    /// <summary>
    /// 批量查询会员信息
    /// </summary>
    Task<IReadOnlyList<MemberInfoContract>> GetMembersInfoAsync(
        IEnumerable<Guid> memberIds);
}
```

##### 2.2 实现查询服务

```csharp
// src/Modules/Members/Infrastructure/MemberQueryService.cs
namespace Zss.BilliardHall.Modules.Members.Infrastructure;

internal sealed class MemberQueryService : IMemberQueryService
{
    private readonly IMemberReadRepository _repository;

    public MemberQueryService(IMemberReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<MemberInfoContract?> GetMemberInfoAsync(Guid memberId)
    {
        var member = await _repository.GetByIdAsync(memberId);
        
        if (member == null)
        {
            return null;
        }

        // 从领域对象映射到契约
        return new MemberInfoContract
        {
            MemberId = member.Id,
            Name = member.Name,
            Email = member.Email,
            MembershipLevel = member.Level.ToString(),
            JoinedAt = member.JoinedAt
        };
    }

    public async Task<IReadOnlyList<MemberInfoContract>> GetMembersInfoAsync(
        IEnumerable<Guid> memberIds)
    {
        var members = await _repository.GetByIdsAsync(memberIds);
        
        return members.Select(m => new MemberInfoContract
        {
            MemberId = m.Id,
            Name = m.Name,
            Email = m.Email,
            MembershipLevel = m.Level.ToString(),
            JoinedAt = m.JoinedAt
        }).ToList();
    }
}
```

**要点**：
- 实现类是 `internal`，只通过接口暴露
- 在内部使用领域对象，但**只返回 DTO**
- 处理空值情况

##### 2.3 注册查询服务

```csharp
// src/Modules/Members/MembersModule.cs
public static class MembersModule
{
    public static IServiceCollection AddMembersModule(
        this IServiceCollection services)
    {
        // 注册查询服务
        services.AddScoped<IMemberQueryService, MemberQueryService>();

        // 其他服务注册...
        
        return services;
    }
}
```

#### 步骤 3：在 Orders 模块中使用查询

##### 3.1 定义组合查询

```csharp
// src/Modules/Orders/UseCases/GetOrderDetails/GetOrderDetails.cs
namespace Zss.BilliardHall.Modules.Orders.UseCases.GetOrderDetails;

/// <summary>
/// 获取订单详情（包含会员信息）
/// </summary>
public sealed record GetOrderDetails(Guid OrderId) : IQuery<OrderDetailsDto>;
```

##### 3.2 定义返回 DTO

```csharp
// src/Modules/Orders/UseCases/GetOrderDetails/OrderDetailsDto.cs
namespace Zss.BilliardHall.Modules.Orders.UseCases.GetOrderDetails;

public sealed record OrderDetailsDto
{
    public Guid OrderId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public OrderStatus Status { get; init; }
    
    // 嵌入的会员信息（来自契约查询）
    public MemberInfoDto? MemberInfo { get; init; }
}

public sealed record MemberInfoDto
{
    public Guid MemberId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MembershipLevel { get; init; } = string.Empty;
}
```

##### 3.3 实现查询 Handler

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

        // 2. 跨模块查询会员信息（通过契约）
        var memberInfo = await _memberQueryService
            .GetMemberInfoAsync(order.MemberId);

        // 3. 组合返回 DTO
        return new OrderDetailsDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            
            // 映射会员信息
            MemberInfo = memberInfo != null ? new MemberInfoDto
            {
                MemberId = memberInfo.MemberId,
                Name = memberInfo.Name,
                Email = memberInfo.Email,
                MembershipLevel = memberInfo.MembershipLevel
            } : null
        };
    }
}
```

**要点**：
- 注入 `IMemberQueryService`（来自 Members 模块）
- 只用于**查询和展示**，不用于业务决策
- 优雅处理会员信息缺失的情况

---

### 测试验证

#### 单元测试：验证查询组合

```csharp
// src/tests/Modules.Orders.Tests/UseCases/GetOrderDetails/GetOrderDetailsHandlerTests.cs
public class GetOrderDetailsHandlerTests
{
    [Fact]
    public async Task Handle_OrderExists_ReturnsDetailsWithMemberInfo()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        
        var orderRepository = Substitute.For<IOrderRepository>();
        var memberQueryService = Substitute.For<IMemberQueryService>();
        
        var order = new Order(memberId, items: []);
        orderRepository.GetByIdAsync(orderId).Returns(order);
        
        var memberInfo = new MemberInfoContract
        {
            MemberId = memberId,
            Name = "张三",
            Email = "zhang@example.com",
            MembershipLevel = "Gold"
        };
        memberQueryService.GetMemberInfoAsync(memberId).Returns(memberInfo);
        
        var handler = new GetOrderDetailsHandler(
            orderRepository,
            memberQueryService
        );
        
        var query = new GetOrderDetails(orderId);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.MemberInfo.Should().NotBeNull();
        result.MemberInfo!.Name.Should().Be("张三");
        result.MemberInfo.MembershipLevel.Should().Be("Gold");
    }

    [Fact]
    public async Task Handle_MemberNotFound_ReturnsOrderWithNullMemberInfo()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        
        var orderRepository = Substitute.For<IOrderRepository>();
        var memberQueryService = Substitute.For<IMemberQueryService>();
        
        var order = new Order(memberId, items: []);
        orderRepository.GetByIdAsync(orderId).Returns(order);
        
        memberQueryService.GetMemberInfoAsync(memberId)
            .Returns((MemberInfoContract?)null);
        
        var handler = new GetOrderDetailsHandler(
            orderRepository,
            memberQueryService
        );
        
        var query = new GetOrderDetails(orderId);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.MemberInfo.Should().BeNull();
    }
}
```

#### 集成测试：端到端验证

```csharp
// src/tests/IntegrationTests/Queries/GetOrderDetailsIntegrationTests.cs
[Collection("Integration")]
public class GetOrderDetailsIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public GetOrderDetailsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsCompleteData()
    {
        // Arrange - 创建会员
        var memberId = await _fixture.SendAsync(new RegisterMember(
            Name: "李四",
            Email: "li@example.com"
        ));

        // Arrange - 创建订单
        var orderId = await _fixture.SendAsync(new CreateOrder(
            MemberId: memberId,
            Items: new[] { new OrderItem("product1", 1, 100m) }
        ));

        // Act - 查询订单详情
        var query = new GetOrderDetails(orderId);
        var result = await _fixture.QueryAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.MemberInfo.Should().NotBeNull();
        result.MemberInfo!.MemberId.Should().Be(memberId);
        result.MemberInfo.Name.Should().Be("李四");
    }
}
```

---

## 常见陷阱

### ❌ 陷阱 1：用契约查询的数据做业务决策

```csharp
// ❌ 错误：在 Command Handler 中使用契约做业务逻辑
public async Task<Guid> Handle(PlaceOrder command)
{
    // 查询会员信息
    var memberInfo = await _memberQueryService
        .GetMemberInfoAsync(command.MemberId);
    
    // ❌ 不要用契约数据做业务决策
    if (memberInfo.MembershipLevel == "Gold")
    {
        // 应用折扣逻辑...
    }
    
    // 创建订单...
}
```

**问题**（根据 ADR-005）：
- 契约查询只能用于**展示**，不能用于**业务决策**
- 业务逻辑应该在领域模型内

**正确做法**：
```csharp
// ✅ 方案 1：通过事件通知
// Members 模块发布 MemberLevelChangedEvent
// Orders 模块订阅并缓存必要的信息

// ✅ 方案 2：保存原始类型
public async Task<Guid> Handle(PlaceOrder command)
{
    // 只保存 MemberId，不查询会员详情
    var order = new Order(
        memberId: command.MemberId,  // 原始类型
        items: command.Items
    );
    
    // 业务逻辑在 Order 领域对象内
    order.ApplyMemberDiscount();
    
    await _repository.SaveAsync(order);
    return order.Id;
}
```

### ❌ 陷阱 2：查询服务中包含业务逻辑

```csharp
// ❌ 错误：查询服务不应包含业务逻辑
public async Task<MemberInfoContract?> GetMemberInfoAsync(Guid memberId)
{
    var member = await _repository.GetByIdAsync(memberId);
    
    // ❌ 不要在查询服务中做业务逻辑
    if (member.Points > 1000)
    {
        member.UpgradeLevel();  // 修改状态
        await _repository.SaveAsync(member);
    }
    
    return MapToContract(member);
}
```

**问题**：
- 查询服务应该是**只读**的
- 业务逻辑应该在 Command Handler 中

**正确做法**：
```csharp
// ✅ 正确：查询服务只读取和映射
public async Task<MemberInfoContract?> GetMemberInfoAsync(Guid memberId)
{
    var member = await _repository.GetByIdAsync(memberId);
    
    if (member == null)
    {
        return null;
    }
    
    return MapToContract(member);
}
```

### ❌ 陷阱 3：返回过多不必要的数据

```csharp
// ❌ 错误：返回所有领域对象的数据
public sealed record MemberInfoContract
{
    public Guid MemberId { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string PasswordHash { get; init; }      // ❌ 敏感信息
    public List<Address> Addresses { get; init; }   // ❌ 可能不需要
    public List<Order> OrderHistory { get; init; }  // ❌ 跨模块数据
    // ... 100 个字段
}
```

**问题**：
- 暴露了敏感信息
- 包含不必要的关联数据
- 契约过大影响性能

**正确做法**：
```csharp
// ✅ 正确：只包含必要的展示数据
public sealed record MemberInfoContract
{
    public Guid MemberId { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string MembershipLevel { get; init; }
    public DateTime JoinedAt { get; init; }
}
```

---

## 最佳实践

### ✅ 实践 1：契约版本化

当契约需要演进时，使用版本化避免破坏现有消费者：

```csharp
// V1 契约
namespace Zss.BilliardHall.BuildingBlocks.Contracts.Members.V1;
public sealed record MemberInfoContract { ... }

// V2 契约（新增字段）
namespace Zss.BilliardHall.BuildingBlocks.Contracts.Members.V2;
public sealed record MemberInfoContract 
{ 
    // V1 字段...
    public string PhoneNumber { get; init; }  // 新增
}
```

### ✅ 实践 2：批量查询优化

当需要查询多个对象时，提供批量接口：

```csharp
public interface IMemberQueryService
{
    // 单个查询
    Task<MemberInfoContract?> GetMemberInfoAsync(Guid memberId);
    
    // ✅ 批量查询（避免 N+1 问题）
    Task<IReadOnlyList<MemberInfoContract>> GetMembersInfoAsync(
        IEnumerable<Guid> memberIds);
}
```

### ✅ 实践 3：缓存契约查询结果

对于频繁访问的数据，考虑缓存：

```csharp
public sealed class CachedMemberQueryService : IMemberQueryService
{
    private readonly IMemberQueryService _inner;
    private readonly IDistributedCache _cache;

    public async Task<MemberInfoContract?> GetMemberInfoAsync(Guid memberId)
    {
        var cacheKey = $"member:{memberId}";
        
        // 尝试从缓存读取
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<MemberInfoContract>(cached);
        }
        
        // 缓存未命中，查询数据库
        var result = await _inner.GetMemberInfoAsync(memberId);
        
        if (result != null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
                });
        }
        
        return result;
    }
}
```

---

## 架构合规检查清单

根据 ADR-001 和 ADR-005，确认：

- [ ] 契约定义在 BuildingBlocks，不暴露领域对象
- [ ] 查询服务接口是只读的（无修改方法）
- [ ] 查询结果只用于展示，不用于业务决策
- [ ] Command Handler 不依赖契约做业务逻辑
- [ ] 契约只包含必要的展示数据
- [ ] 查询服务实现是 internal 的
- [ ] 架构测试通过（无不当的跨模块依赖）

---

## 参考资料

- [ADR-001：模块化单体与垂直切片架构](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md) - 第 2.2 节：模块通信规则
- [ADR-005：应用内交互模型与执行边界](../adr/constitutional/ADR-005-Application-Interaction-Model-Final.md) - 第 2.2 节：Query Handler 规则
- [模块化架构 FAQ](../faqs/architecture-faq.md) - Q: 模块间如何通信？
- [跨模块通信指南](../guides/cross-module-communication.md)

---

## 相关案例

- [领域事件通信模式](domain-event-communication-case.md) - 跨模块异步通信
- [Handler 单元测试](handler-unit-testing-case.md) - 测试查询逻辑

---

**维护**：Tech Lead  
**状态**：✅ Active  
**审核**: 已通过架构委员会审查（2026-01-27）  
**最后更新**：2026-01-27
